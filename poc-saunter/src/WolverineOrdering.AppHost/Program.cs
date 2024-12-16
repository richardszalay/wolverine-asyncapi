var builder = DistributedApplication.CreateBuilder(args);

var kafka = builder.AddKafka("kafka")
    .WithKafkaUI();

builder.AddProject<Projects.WolverineOrdering_OrderApi>("wolverineordering-orderapi")
    .WithReference(kafka);

builder.AddProject<Projects.WolverineOrdering_FulfilmentApi>("wolverineordering-fulfilmentapi")
    .WithReference(kafka);

builder.Build().Run();
