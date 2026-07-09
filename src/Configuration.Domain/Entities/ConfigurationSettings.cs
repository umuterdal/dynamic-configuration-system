namespace Configuration.Domain.Entities;

/// <summary>
/// Configuration settings for the ConfigurationReader.
/// Used with Options Pattern for dependency injection.
/// </summary>
public sealed class ConfigurationSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "ConfigurationReader";

    /// <summary>
    /// The name of the application this reader serves.
    /// Used for service-level isolation.
    /// </summary>
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// MongoDB connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Database name in MongoDB.
    /// </summary>
    public string DatabaseName { get; set; } = "ConfigurationDb";

    /// <summary>
    /// Collection name in MongoDB.
    /// </summary>
    public string CollectionName { get; set; } = "Configurations";

    /// <summary>
    /// Refresh interval in milliseconds for background service.
    /// </summary>
    public int RefreshTimerIntervalInMs { get; set; } = 30000;
}
