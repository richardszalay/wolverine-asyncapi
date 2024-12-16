POC integration between [Wolverine](https://github.com/JasperFx/wolverine) and [Saunter](https://github.com/asyncapi/saunter)

To run: `dotnet run --project .\src\WolverineOrdering.AppHost\WolverineOrdering.AppHost.csproj`

AsyncAPI UI should be available via the following URLs (ports may change, check each APIs console logs via Aspire):

Order API UI:
http://localhost:5089/asyncapi/ui/index.html

Order API JSON:
http://localhost:5089/asyncapi/asyncapi.json

Fulfilment API UI:
http://localhost:5033/asyncapi/ui/index.html

Fulfilment API JSON:
http://localhost:5033/asyncapi/asyncapi.json