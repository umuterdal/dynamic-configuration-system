using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using Configuration.Library.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

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

            // Create MongoDB client for health checks
            IMongoClient? mongoClient = null;
            if (!string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                var mongoSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
                mongoSettings.ServerApi = new ServerApi(ServerApiVersion.V1);
                mongoClient = new MongoClient(mongoSettings);
            }

            return new ConfigurationReader(
                settings.ApplicationName,
                repository,
                settings.RefreshTimerIntervalInMs,
                logger,
                mongoClient,
                settings.DatabaseName);
        });

        return services;
    }

    /// <summary>
    /// Adds the ConfigurationBrokerConsumer background service.
    /// Enables instant cache refresh via RabbitMQ messages.
    /// Polling continues as the primary mechanism.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConfigurationBrokerConsumer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ConfigurationBrokerSettings>(
            configuration.GetSection(ConfigurationBrokerSettings.SectionName));

        services.AddHostedService<ConfigurationBrokerConsumer>();

        return services;
    }
}
