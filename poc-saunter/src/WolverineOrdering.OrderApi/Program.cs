using Microsoft.AspNetCore.Routing.Constraints;
using Oakton;
using Oakton.Resources;
using Saunter;
using Wolverine;
using Wolverine.Http;
using Wolverine.Kafka;
using WolverineOrdering.Fulfilment.Contract;
using WolverineOrdering.Order.Contract;
using WolverineOrdering.OrderApi;
using Wolverine.AsyncApi.Saunter;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddAsyncApiSchemaGeneration(opt =>
{
    var applicationName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME");
    opt.AsyncApi.Id = $"http://tempuri.org/";
    opt.AsyncApi.Info = new(applicationName, "1.0");
});

builder.Services.AddWolverineAsyncApiSchemaGeneration();

builder.Services.AddResourceSetupOnStartup();

builder.WebHost.UseKestrelHttpsConfiguration();

// Wolverine usage is required for WolverineFx.Http
builder.Host.UseWolverine(opts =>
{
    opts.UseKafka(builder.Configuration.GetConnectionString("kafka")!);
    
    opts.ListenToKafkaTopic(FulfilmentTopics.Fulfilment)
        .BufferedInMemory();

    opts.ListenToKafkaTopic(OrderTopics.Orders)
        .BufferedInMemory();

    opts.Publish(p => p.MessagesFromAssembly(typeof(FulfilmentTopics).Assembly)
        .ToKafkaTopic(FulfilmentTopics.Fulfilment));

    opts.Publish(p => p.MessagesFromAssembly(typeof(OrderTopics).Assembly)
        .ToKafkaTopic(OrderTopics.Orders));
});

// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2951
builder.Services.Configure<RouteOptions>(
    options => options.SetParameterPolicy<RegexInlineRouteConstraint>("regex"));

builder.Services.AddWolverineHttp();

var app = builder.Build();

app.UseRouting();

app.MapWolverineEndpoints();
app.MapAsyncApiDocuments();
app.MapAsyncApiUi();

return await app.RunOaktonCommands(args);