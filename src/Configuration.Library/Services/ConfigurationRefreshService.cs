using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Configuration.Library.Services;

/// <summary>
/// Background service that periodically refreshes configuration cache.
/// Uses PeriodicTimer for efficient and predictable scheduling.
/// </summary>
public sealed class ConfigurationRefreshService : BackgroundService
{
    private readonly IConfigurationRepository _repository;
    private readonly ILogger<ConfigurationRefreshService> _logger;
    private readonly ConfigurationSettings _settings;
    private readonly TimeSpan _refreshInterval;

    /// <summary>
    /// Initializes a new instance of the ConfigurationRefreshService.
    /// </summary>
    /// <param name="repository">The configuration repository.</param>
    /// <param name="settings">Configuration settings.</param>
    /// <param name="logger">Logger instance.</param>
    public ConfigurationRefreshService(
        IConfigurationRepository repository,
        IOptions<ConfigurationSettings> settings,
        ILogger<ConfigurationRefreshService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
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
                    await RefreshConfigurationsAsync(stoppingToken);
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

    private async Task RefreshConfigurationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Refreshing configurations for application {ApplicationName}",
            _settings.ApplicationName);

        var records = await _repository.GetByApplicationNameAsync(
            _settings.ApplicationName, cancellationToken);

        _logger.LogDebug(
            "Refreshed {Count} configurations for application {ApplicationName}",
            records.Count, _settings.ApplicationName);
    }
}
