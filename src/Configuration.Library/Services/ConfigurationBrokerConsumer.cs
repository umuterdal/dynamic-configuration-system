using System.Text;
using System.Text.Json;
using Configuration.Domain.Entities;
using Configuration.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Configuration.Library.Services;

/// <summary>
/// Background service that consumes configuration change events from RabbitMQ.
/// Triggers immediate cache refresh when a change is detected.
/// Polling continues as the primary mechanism; this improves latency.
/// </summary>
public sealed class ConfigurationBrokerConsumer : BackgroundService
{
    private readonly ConfigurationReader _configurationReader;
    private readonly ILogger<ConfigurationBrokerConsumer> _logger;
    private readonly ConfigurationBrokerSettings _settings;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the ConfigurationBrokerConsumer.
    /// </summary>
    /// <param name="configurationReader">The configuration reader to refresh.</param>
    /// <param name="settings">Broker connection settings.</param>
    /// <param name="logger">Logger instance.</param>
    public ConfigurationBrokerConsumer(
        ConfigurationReader configurationReader,
        IOptions<ConfigurationBrokerSettings> settings,
        ILogger<ConfigurationBrokerConsumer> logger)
    {
        _configurationReader = configurationReader ?? throw new ArgumentNullException(nameof(configurationReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Configuration broker consumer starting, connecting to {Host}:{Port}",
            _settings.HostName, _settings.Port);

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false);

            var queueName = _channel.QueueDeclare(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false).QueueName;

            _channel.QueueBind(
                queue: queueName,
                exchange: _settings.ExchangeName,
                routingKey: string.Empty);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<JsonElement>(json);

                    var changeType = message.GetProperty("ChangeType").GetString();
                    var applicationName = message.GetProperty("ApplicationName").GetString();

                    _logger.LogInformation(
                        "Received config change: {ChangeType} for {ApplicationName}",
                        changeType, applicationName);

                    // Trigger immediate refresh
                    await _configurationReader.RefreshAsync();

                    _logger.LogInformation(
                        "Cache refreshed after broker event: {ChangeType} for {ApplicationName}",
                        changeType, applicationName);

                    // Acknowledge the message
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing broker message");
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation(
                "Configuration broker consumer started, queue: {Queue}",
                queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to connect to RabbitMQ at {Host}:{Port}. " +
                "Polling will continue as primary refresh mechanism.",
                _settings.HostName, _settings.Port);
        }

        // Keep the service running
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override void Dispose()
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
            _logger.LogWarning(ex, "Error disposing broker consumer");
        }

        base.Dispose();
    }
}
