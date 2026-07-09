using System.Text;
using System.Text.Json;
using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Configuration.Application.Services;

/// <summary>
/// RabbitMQ implementation of IConfigurationBrokerPublisher.
/// Publishes configuration change events to notify consumers for instant cache refresh.
/// </summary>
public sealed class RabbitMqConfigurationBrokerPublisher : IConfigurationBrokerPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName;
    private readonly ILogger<RabbitMqConfigurationBrokerPublisher> _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the RabbitMqConfigurationBrokerPublisher.
    /// </summary>
    /// <param name="settings">Broker connection settings.</param>
    /// <param name="logger">Logger instance.</param>
    public RabbitMqConfigurationBrokerPublisher(
        ConfigurationBrokerSettings settings,
        ILogger<RabbitMqConfigurationBrokerPublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        _exchangeName = settings.ExchangeName;

        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: _exchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false);

        _logger.LogInformation(
            "RabbitMQ publisher connected to {Host}:{Port}, exchange: {Exchange}",
            settings.HostName, settings.Port, _exchangeName);
    }

    /// <inheritdoc />
    public Task PublishAsync(string applicationName, string changeType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
            throw new ArgumentException("Application name cannot be null or empty.", nameof(applicationName));

        if (string.IsNullOrWhiteSpace(changeType))
            throw new ArgumentException("Change type cannot be null or empty.", nameof(changeType));

        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var message = new
            {
                ApplicationName = applicationName,
                ChangeType = changeType,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = Guid.NewGuid().ToString("N");
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: string.Empty,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Published config change: {ChangeType} for {ApplicationName}",
                changeType, applicationName);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish config change for application {ApplicationName}",
                applicationName);

            // Don't throw — broker failure should not break the application
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing RabbitMQ publisher");
        }
    }
}
