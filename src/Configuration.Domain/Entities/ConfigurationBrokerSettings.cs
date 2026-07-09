namespace Configuration.Domain.Entities;

/// <summary>
/// Configuration for the message broker connection.
/// </summary>
public sealed class ConfigurationBrokerSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "ConfigurationBroker";

    /// <summary>
    /// RabbitMQ host name.
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ port.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ user name.
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ password.
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Exchange name for configuration change events.
    /// </summary>
    public string ExchangeName { get; set; } = "config-changes";

    /// <summary>
    /// Queue name for this consumer.
    /// </summary>
    public string QueueName { get; set; } = "config-change-consumer";
}
