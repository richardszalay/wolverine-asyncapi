using JasperFx.CodeGeneration.Frames;
using Microsoft.Extensions.Options;
using Saunter;
using Saunter.AsyncApiSchema.v2;
using Saunter.Attributes;
using System.Reflection;
using System.Runtime.CompilerServices;
using Wolverine.Runtime;
using Wolverine.Runtime.Handlers;
using Wolverine.Runtime.Routing;

namespace Wolverine.AsyncApi.Saunter;

internal class WolverineAsyncApiDocumentProvider : IAsyncApiDocumentProvider
{
    private readonly HandlerGraph wolverineHandlerGraph;
    private readonly IWolverineRuntime wolverineRuntime;
    private readonly WolverineAsyncApiOptions options;

    public WolverineAsyncApiDocumentProvider(HandlerGraph wolverineHandlerGraph, IWolverineRuntime wolverineRuntime, IOptions<WolverineAsyncApiOptions> options)
    {
        this.wolverineHandlerGraph = wolverineHandlerGraph;
        this.wolverineRuntime = wolverineRuntime;
        this.options = options.Value;
    }

    public AsyncApiDocument GetDocument(AsyncApiOptions options, AsyncApiDocument prototype)
    {
        var output = prototype.Clone();

        var listeningChannels = new List<ChannelItem>();

        ChannelItem AddChannel(Uri channelUri)
        {
            if (output.Channels.TryGetValue(channelUri.AbsoluteUri, out var existingChannel))
            {
                return existingChannel;
            }

            var channel = new ChannelItem()
            {
                Subscribe = new Operation()
                {
                    Message = new Messages()
                },
                Publish = new Operation()
                {
                    Message = new Messages()
                },
                Servers = new List<string>
                {
                }
            };

            output.Channels.Add(channelUri.AbsoluteUri, channel);

            return channel;
        }

        // We don't have enough information to define servers from transports
        /*foreach (var transport in wolverineRuntime.Options.Transports)
        {
            if (transport is not IBrokerTransport)
                continue;

            output.Servers[transport.Protocol] = new Server(
                new UriBuilder(transport.Protocol, transport.Protocol).Uri.AbsoluteUri,
                transport.Protocol
            );
        }*/

        foreach (var listener in wolverineRuntime.Endpoints.ActiveListeners())
        {
            var channel = AddChannel(listener.Uri);
            listeningChannels.Add(channel);
        }

        foreach (var handlerChain in wolverineHandlerGraph.Chains)
        {
            foreach (var handler in handlerChain.Handlers)
            {
                var receiveMessageReference = AddMessageComponent(output, handlerChain.MessageType);

                // We currently have no way of knowing which channel each message is intended to come from,
                // so they have to be added to all of them
                foreach (var messagingChannel in listeningChannels)
                {
                    ((Messages)messagingChannel.Subscribe.Message).OneOf.Add(receiveMessageReference);
                }

                foreach (var cascadingMessageType in GetCascadingMessageTypes(handler))
                {
                    var sendMessageReference = AddMessageComponent(output, cascadingMessageType);

                    // Unlike listeners, we can add the message to the specific channel
                    var routes = GetMessageRoutes(wolverineRuntime.RoutingFor(cascadingMessageType));

                    foreach (var messageRoute in routes)
                    {
                        var targetUri = messageRoute.Sender.Destination;

                        var channel = AddChannel(targetUri);

                        ((Messages)channel.Publish.Message).OneOf.Add(sendMessageReference);
                    }
                }
            }
        }

        return output;
    }

    private IEnumerable<MessageRoute> GetMessageRoutes(IMessageRouter input)
    {
        foreach (var route in input.Routes)
        {
            if (route is MessageRoute messageRoute)
            {
                yield return messageRoute;
            }
            else if (route is IMessageRouter childRouter)
            {
                foreach (var output in GetMessageRoutes(childRouter))
                {
                    yield return output;
                }
            }
        }
    }

    // Currently ignores runtime-dynamic types (IEnumerable<object>, OutgoingMessages)
    private IEnumerable<Type> GetCascadingMessageTypes(MethodCall handler)
    {
        if (handler.ReturnType is null)
        {
            yield break;
        }

        var publishAttributes = handler.Method.GetCustomAttributes<PublishOperationAttribute>().ToList();

        if (handler.ReturnType.IsAssignableTo(typeof(ITuple)))
        {
            var tupleTypes = handler.ReturnType.GetGenericArguments();

            foreach (var type in tupleTypes)
            {
                if (IsCascadingMessageType(type))
                {
                    yield return type;
                }
            }
        }
        else if (IsCascadingMessageType(handler.ReturnType))
        {
            yield return handler.ReturnType;
        }
    }

    private bool IsCascadingMessageType(Type type)
    {
        var isSideEffect = type.IsAssignableTo(typeof(ISideEffect));

        // This POC has not approached how runtime dynamic message types could be specified
        // (i.e. via an attribute, code comments, etc)
        var isRuntimeDymamic = type == typeof(IEnumerable<object>) ||
            type == typeof(OutgoingMessages);

        return !(isSideEffect || isRuntimeDymamic);
    }

    private MessageReference AddMessageComponent(AsyncApiDocument output, Type messageType)
    {
        var typeName = messageType.Name;

        if (output.Components.Messages.ContainsKey(typeName))
        {
            return new MessageReference(typeName);
        }

        var message = new Message()
        {
            //MessageId = messageType.Name,
            ContentType = "application/json",
            // Title, Description, Summmary TODO get from attribute / code comment?
            Payload = NJsonSchema.JsonSchema.FromType(messageType)
        };

        output.Components.Messages.Add(messageType.Name, message);

        return new MessageReference(typeName);
    }
}
