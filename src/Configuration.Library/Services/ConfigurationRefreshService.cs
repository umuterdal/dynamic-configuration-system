using Configuration.Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Configuration.Library.Services;

/// <summary>
/// Background service that periodically refreshes configuration cache.
/// Uses PeriodicTimer for efficient and predictable scheduling.
/// Calls ConfigurationReader.RefreshAsync() to update the in-memory cache.
/// </summary>
public sealed class ConfigurationRefreshService : BackgroundService
{
    private readonly ConfigurationReader _configurationReader;
    private readonly ILogger<ConfigurationRefreshService> _logger;
    private readonly ConfigurationSettings _settings;
    private readonly TimeSpan _refreshInterval;

    /// <summary>
    /// Initializes a new instance of the ConfigurationRefreshService.
    /// </summary>
    /// <param name="configurationReader">The configuration reader to refresh.</param>
    /// <param name="settings">Configuration settings.</param>
    /// <param name="logger">Logger instance.</param>
    public ConfigurationRefreshService(
        ConfigurationReader configurationReader,
        IOptions<ConfigurationSettings> settings,
        ILogger<ConfigurationRefreshService> logger)
    {
        _configurationReader = configurationReader ?? throw new ArgumentNullException(nameof(configurationReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        if (_settings.RefreshTimerIntervalInMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(settings), "Refresh interval must be positive.");

        _refreshInterval = TimeSpan.FromMilliseconds(_settings.RefreshTimerIntervalInMs);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Configuration refresh service starting for application {ApplicationName} with {Interval}ms interval",
            _settings.ApplicationName, _settings.RefreshTimerIntervalInMs);

        using var timer = new PeriodicTimer(_refreshInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    await _configurationReader.RefreshAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Expected during shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error during configuration refresh for application {ApplicationName}",
                        _settings.ApplicationName);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }

        _logger.LogInformation(
            "Configuration refresh service stopping for application {ApplicationName}",
            _settings.ApplicationName);
    }
}
