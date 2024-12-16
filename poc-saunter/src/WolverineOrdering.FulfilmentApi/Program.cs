using Oakton;
using Oakton.Resources;
using Saunter;
using Wolverine;
using Wolverine.AsyncApi.Saunter;
using Wolverine.Kafka;
using WolverineOrdering.Fulfilment.Contract;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddResourceSetupOnStartup();

builder.Host.UseWolverine(opts =>
{
    opts.UseKafka(builder.Configuration.GetConnectionString("kafka")!);

    opts.ListenToKafkaTopic(FulfilmentTopics.Fulfilment)
        .BufferedInMemory();

    opts.Publish(p =>
    {
        p.MessagesFromAssembly(typeof(FulfilmentTopics).Assembly)
            .ToKafkaTopic(FulfilmentTopics.Fulfilment);
    });
});

builder.Services.AddAsyncApiSchemaGeneration(opt =>
{
    var applicationName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME");
    opt.AsyncApi.Id = $"http://tempuri.org/";
    opt.AsyncApi.Info = new(applicationName, "1.0");
});

builder.Services.AddWolverineAsyncApiSchemaGeneration();

var app = builder.Build();

app.MapAsyncApiDocuments();
app.MapAsyncApiUi();

return await app.RunOaktonCommands(args);
