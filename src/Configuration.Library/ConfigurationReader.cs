using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using Configuration.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;

namespace Configuration.Library;

/// <summary>
/// Thread-safe configuration reader with in-memory cache.
/// Accepts connectionString directly — creates its own MongoDB repository internally.
/// Refresh is handled externally by ConfigurationRefreshService (BackgroundService).
/// </summary>
public sealed class ConfigurationReader : IDisposable
{
    private readonly string _applicationName;
    private readonly IConfigurationRepository _repository;
    private readonly ILogger<ConfigurationReader> _logger;
    private ConcurrentDictionary<string, ConfigurationRecord> _cache;
    private int _refreshing;
    private bool _disposed;

    private const string DefaultDatabaseName = "ConfigurationDb";
    private const string DefaultCollectionName = "Configurations";

    /// <summary>
    /// Initializes a new instance of the ConfigurationReader.
    /// Creates its own MongoDB connection and repository internally.
    /// Loads initial configuration from the repository.
    /// </summary>
    /// <param name="applicationName">The application name for service isolation.</param>
    /// <param name="connectionString">MongoDB connection string.</param>
    /// <param name="refreshTimerIntervalInMs">Refresh interval in milliseconds (used by BackgroundService).</param>
    public ConfigurationReader(
        string applicationName,
        string connectionString,
        int refreshTimerIntervalInMs)
    {
        _applicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        if (refreshTimerIntervalInMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(refreshTimerIntervalInMs), "Refresh interval must be positive.");

        // Create MongoDB connection and repository internally
        var mongoSettings = MongoClientSettings.FromConnectionString(connectionString);
        mongoSettings.ServerApi = new ServerApi(ServerApiVersion.V1);

        var client = new MongoClient(mongoSettings);
        var database = client.GetDatabase(DefaultDatabaseName);
        var collection = database.GetCollection<ConfigurationRecord>(DefaultCollectionName);

        var repoLogger = NullLogger<MongoConfigurationRepository>.Instance;
        _repository = new MongoConfigurationRepository(collection, repoLogger);

        _logger = NullLogger<ConfigurationReader>.Instance;
        _cache = new ConcurrentDictionary<string, ConfigurationRecord>(StringComparer.OrdinalIgnoreCase);

        // Initial load - synchronous to ensure cache is populated before first access
        LoadConfigurationsAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initializes a new instance of the ConfigurationReader.
    /// Used when repository and logger are provided via dependency injection.
    /// </summary>
    /// <param name="applicationName">The application name for service isolation.</param>
    /// <param name="repository">The configuration repository for data access.</param>
    /// <param name="refreshTimerIntervalInMs">Refresh interval in milliseconds (used by BackgroundService).</param>
    /// <param name="logger">Logger instance.</param>
    internal ConfigurationReader(
        string applicationName,
        IConfigurationRepository repository,
        int refreshTimerIntervalInMs,
        ILogger<ConfigurationReader> logger)
    {
        _applicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = new ConcurrentDictionary<string, ConfigurationRecord>(StringComparer.OrdinalIgnoreCase);

        if (refreshTimerIntervalInMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(refreshTimerIntervalInMs), "Refresh interval must be positive.");

        // Initial load - synchronous to ensure cache is populated before first access
        LoadConfigurationsAsync().GetAwaiter().GetResult();

        _logger.LogInformation(
            "ConfigurationReader initialized for application {ApplicationName} with {Interval}ms refresh interval",
            _applicationName, refreshTimerIntervalInMs);
    }

    /// <summary>
    /// Gets a configuration value by key with type conversion.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="key">The configuration key.</param>
    /// <returns>The converted configuration value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when type conversion fails.</exception>
    public T GetValue<T>(string key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        if (!_cache.TryGetValue(key, out var record))
        {
            _logger.LogWarning("Configuration key {Key} not found for application {ApplicationName}",
                key, _applicationName);
            throw new KeyNotFoundException($"Configuration key '{key}' not found for application '{_applicationName}'.");
        }

        if (record.IsActive != 1)
        {
            _logger.LogWarning("Configuration key {Key} is inactive for application {ApplicationName}",
                key, _applicationName);
            throw new KeyNotFoundException($"Configuration key '{key}' is inactive for application '{_applicationName}'.");
        }

        return ConvertValue<T>(record.Value, record.Type, key);
    }

    /// <summary>
    /// Tries to get a configuration value by key.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The converted value if found.</param>
    /// <returns>True if key exists and conversion succeeded; otherwise, false.</returns>
    public bool TryGetValue<T>(string key, out T? value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (string.IsNullOrWhiteSpace(key) || !_cache.TryGetValue(key, out var record) || record.IsActive != 1)
        {
            value = default;
            return false;
        }

        try
        {
            value = ConvertValue<T>(record.Value, record.Type, key);
            return true;
        }
        catch (InvalidCastException)
        {
            value = default;
            return false;
        }
        catch (FormatException)
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Gets all active configuration values as a read-only dictionary.
    /// </summary>
    /// <returns>Dictionary of key-value pairs.</returns>
    public IReadOnlyDictionary<string, string> GetAllValues()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _cache
            .Where(kvp => kvp.Value.IsActive == 1)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Forces an immediate refresh of the configuration cache.
    /// Thread-safe: concurrent calls are coalesced into a single refresh.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RefreshAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Prevent overlapping refreshes using interlocked compare-exchange
        if (Interlocked.CompareExchange(ref _refreshing, 1, 0) != 0)
            return;

        try
        {
            await RefreshConfigurationsAsync();
        }
        finally
        {
            Interlocked.Exchange(ref _refreshing, 0);
        }
    }

    private async Task LoadConfigurationsAsync()
    {
        try
        {
            _logger.LogDebug("Loading configurations for application {ApplicationName}", _applicationName);

            var records = await _repository.GetByApplicationNameAsync(_applicationName);

            // Build new cache content
            var newCache = new ConcurrentDictionary<string, ConfigurationRecord>(StringComparer.OrdinalIgnoreCase);
            foreach (var record in records)
            {
                newCache[record.Name] = record;
            }

            // Atomic swap: replace the entire cache reference
            Interlocked.Exchange(ref _cache, newCache);

            _logger.LogInformation(
                "Loaded {Count} active configurations for application {ApplicationName}",
                records.Count, _applicationName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to load configurations for application {ApplicationName}. " +
                "Cache will retain previous values if available.",
                _applicationName);
        }
    }

    private async Task RefreshConfigurationsAsync()
    {
        if (_disposed) return;

        try
        {
            _logger.LogDebug("Refreshing configurations for application {ApplicationName}", _applicationName);

            var records = await _repository.GetByApplicationNameAsync(_applicationName);

            // Build new cache content
            var newCache = new ConcurrentDictionary<string, ConfigurationRecord>(StringComparer.OrdinalIgnoreCase);
            foreach (var record in records)
            {
                newCache[record.Name] = record;
            }

            // Atomic swap: replace the entire cache reference
            Interlocked.Exchange(ref _cache, newCache);

            _logger.LogDebug(
                "Refreshed {Count} configurations for application {ApplicationName}",
                records.Count, _applicationName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to refresh configurations for application {ApplicationName}. " +
                "Cache will retain previous values.",
                _applicationName);
        }
    }

    private static T ConvertValue<T>(string value, string type, string key)
    {
        var targetType = typeof(T);

        try
        {
            if (targetType == typeof(string))
            {
                return (T)(object)value;
            }

            if (string.Equals(type, "int", StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)int.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (string.Equals(type, "double", StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)double.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (string.Equals(type, "bool", StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)bool.Parse(value);
            }
            else if (string.Equals(type, "string", StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)value;
            }
            else
            {
                return (T)TypeDescriptor.GetConverter(targetType).ConvertFromString(value)!;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidCastException(
                $"Failed to convert value '{value}' to type {targetType.Name} for key '{key}'.", ex);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
