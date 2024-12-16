A proof-of-concept spike on generating [AsyncAPI](https://www.asyncapi.com/) documents using metadata available from [Wolverine](https://wolverinefx.net/), with the intended use case being to provide documentation about what messages are consumed / produced by a service automatically.

Primary objectives:

1. Determine any gaps or other compatibility issues between what Wolverine can provide and the AsyncAPI document model
2. Understand what AsyncAPI libraries are currently available for .NET

## Findings

### AsyncAPI v2 vs v3

The most relevant change between AsyncAPI v2 and v3 is that operations were separated from channels. In v2, each channel could only contain one "Publish" channel and one "Subscribe" channel.

These channels could list more than one message, but there was no room for documentation specific to that channel-and-message, nor from multiple subscribers to the same channel.

### Model compatibility (Async API v3)

| AsyncAPI    | Wolverine |
| -------- | ------- |
| Server  | Transport |
| Channel | Endpoint |
| Operation(receive)    | Handler messages   |
| Operation(send)    | Handler return values |
| Message | Types |

Despite the reasonably clean mapping, there are practical issues with each:

* The public API for **Transport** does not expose the underlying server details, so the representation is limit to the type of server (RabbitMQ, Kafka, etc)
* As there's no way to associate specific types to a listening endpoint, the mapped receive operations cannot be mapped to channels
* Dynamic handler return values (`IEnumerable<object>`, `OutgoingMessages`) can be mapped
* HTTP operations can't be mapped as such without also referencing Wolverine.Http, which may not be be ideal for some Wolverine users that don't make use of Wolverine.Http

### Library Maturity

There is currently no first party (Microsoft) supported AsyncAPI library like there is for OpenAPI.

The OpenAPI site [lists the following tools available for .NET](https://www.asyncapi.com/tools?techs=.NET%2CASP.NET):

* [Lego/AsyncAPI.NET](https://github.com/LEGO/AsyncAPI.NET) is strictly an object model and JSON/YAML serialization library. It does not yet support AsyncAPI v3, and the [roadmap for is unclear](https://github.com/LEGO/AsyncAPI.NET/issues/167)
* [AsyncAPI/Saunter](https://github.com/asyncapi/saunter) is the only generator library under the AsyncAPI GitHub organization. However, it only supports AsyncAPI v2, claims general API instability pre-1.0, and has an [unreleased migration to Lego/AsyncAPI.NET](https://github.com/asyncapi/saunter/issues/188)
* [neuroglia-io/AsyncApi](https://github.com/neuroglia-io/AsyncApi) is the only other active generator library. It only supports AsyncAPI v2, with [v3 support having no clear roadmap](https://github.com/neuroglia-io/asyncapi/issues/31)
* [yurvon-screamo/AsyncApi.Net.Generator](https://github.com/yurvon-screamo/AsyncApi.Net.Generator) is a fork of Saunter, but was archived in April 2024
* [KnstEventBus](https://github.com/d0972058277/KnstEventBus) was archived in Nov 2023.

This repository contains POC integrations with Saunter.

## Thoughts / Recommendations

At this stage, it's hard to advise investing in official integration with AsyncAPI. However, it could be worth exposing a metadata model that such an integration could eventually use.

### Changes to Wolverine

It could be worthwhile exposing metadata information from Wolverine that is compatible with the goals of AsyncAPI, but without being directly AsyncAPI compatible. Essentially an extension of the 'describe' concept, but with an object model.

It could define:

* Servers/transports with sufficient information
* A new (possibly relevant only to metadata) mechanism for associating Message types with a listener endpoint
* A new (possibly attribute-based) mechanism for defining cascading message types on handlers that use a dynamic mechanism
* Channels, built from both Message handlers and Http endpoints, would avoid the consumer from needing to take dependency on Wolverine.Http

The model could leave room for further specifics to be decided by the consumer by directly exposing the message types / MethodCalls, rather than trying to abstract them.

### Library selection

Since both active libraries either only support AsyncAPI or are still undergoing compatibility-breaking changes (or both), it is probably not worth the time investing into either of them for official support.

Unofficial support (either through preview packages, or not under the banner of Wolverine) could provide a basis for others to customise in their own organisations, without committing to a long-term library choice or extensibility model.