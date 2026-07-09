using Configuration.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Configuration.Application.Extensions;

/// <summary>
/// Extension methods for configuring Application services.
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Adds Application services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IConfigurationService, ConfigurationService>();
        return services;
    }
}
