using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Configuration.Library.Extensions;

/// <summary>
/// Extension methods for configuring the Configuration Library.
/// </summary>
public static class ConfigurationServiceExtensions
{
    /// <summary>
    /// Adds a ConfigurationReader singleton to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConfigurationReader(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ConfigurationSettings>(
            configuration.GetSection(ConfigurationSettings.SectionName));

        services.AddSingleton<ConfigurationReader>(sp =>
        {
            var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ConfigurationSettings>>().Value;
            var repository = sp.GetRequiredService<IConfigurationRepository>();
            var logger = sp.GetRequiredService<ILogger<ConfigurationReader>>();

            return new ConfigurationReader(
                settings.ApplicationName,
                repository,
                settings.RefreshTimerIntervalInMs,
                logger);
        });

        return services;
    }
}
