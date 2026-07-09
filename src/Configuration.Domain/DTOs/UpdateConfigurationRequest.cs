namespace Configuration.Domain.DTOs;

/// <summary>
/// Data Transfer Object for updating an existing configuration record.
/// </summary>
public sealed class UpdateConfigurationRequest
{
    /// <summary>
    /// The record ID to update.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The configuration key name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The data type of the value.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The configuration value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Whether the record is active.
    /// </summary>
    public int IsActive { get; set; } = 1;

    /// <summary>
    /// The application name this configuration belongs to.
    /// </summary>
    public string ApplicationName { get; set; } = string.Empty;
}
