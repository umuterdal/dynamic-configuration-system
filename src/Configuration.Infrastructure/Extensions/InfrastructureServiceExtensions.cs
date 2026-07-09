using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using Configuration.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Configuration.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring Infrastructure services.
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Adds Infrastructure services including MongoDB repository to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ConfigurationSettings>(
            configuration.GetSection(ConfigurationSettings.SectionName));

        services.AddSingleton<IConfigurationRepository, MongoConfigurationRepository>();

        return services;
    }
}
