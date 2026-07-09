using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Configuration.Domain.Entities;

/// <summary>
/// Represents a configuration record stored in the database.
/// Each record contains a key-value pair with metadata for a specific application.
/// </summary>
public sealed class ConfigurationRecord
{
    /// <summary>
    /// Unique identifier for the configuration record.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The configuration key name (e.g., "SiteName", "MaxItemCount").
    /// </summary>
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The data type of the value (e.g., "string", "int", "bool", "double").
    /// </summary>
    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The configuration value stored as string.
    /// Type conversion is handled by the reader.
    /// </summary>
    [BsonElement("value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this configuration record is active.
    /// Only active records (IsActive = 1) should be returned to consumers.
    /// </summary>
    [BsonElement("isActive")]
    public int IsActive { get; set; } = 1;

    /// <summary>
    /// The name of the application this configuration belongs to.
    /// Used for service-level isolation.
    /// </summary>
    [BsonElement("applicationName")]
    public string ApplicationName { get; set; } = string.Empty;
}