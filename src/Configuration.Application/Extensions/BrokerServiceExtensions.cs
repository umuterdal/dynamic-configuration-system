using Configuration.Application.Services;
using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Configuration.Application.Extensions;

/// <summary>
/// Extension methods for configuring broker services.
/// </summary>
public static class BrokerServiceExtensions
{
    /// <summary>
    /// Adds RabbitMQ broker publisher to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConfigurationBroker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = new ConfigurationBrokerSettings();
        configuration.GetSection(ConfigurationBrokerSettings.SectionName).Bind(settings);

        services.Configure<ConfigurationBrokerSettings>(options =>
        {
            options.HostName = settings.HostName;
            options.Port = settings.Port;
            options.UserName = settings.UserName;
            options.Password = settings.Password;
            options.ExchangeName = settings.ExchangeName;
            options.QueueName = settings.QueueName;
        });

        services.AddSingleton<IConfigurationBrokerPublisher>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ConfigurationBrokerSettings>>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RabbitMqConfigurationBrokerPublisher>>();
            return new RabbitMqConfigurationBrokerPublisher(options.Value, logger);
        });

        return services;
    }
}
