using Microsoft.Extensions.DependencyInjection;
using Saunter;

namespace Wolverine.AsyncApi.Saunter;

// Could include options for how to pull documentation (attributes vs code comments), etc
public record WolverineAsyncApiOptions();

public static class WolverineAsyncApiExtensions
{

    /// <summary>
    /// Populates the Saunter AsyncAPI document from Wolverine's configuration. Respects any default values populated in AddAsyncApiSchemaGeneration
    /// </summary>
    public static IServiceCollection AddWolverineAsyncApiSchemaGeneration(this IServiceCollection services, Action<WolverineAsyncApiOptions>? configure = null)
    {
        services.AddTransient<IAsyncApiDocumentProvider, WolverineAsyncApiDocumentProvider>();

        services.Configure<WolverineAsyncApiOptions>(options =>
        {
            configure?.Invoke(options);
        });

        return services;
    }
}
