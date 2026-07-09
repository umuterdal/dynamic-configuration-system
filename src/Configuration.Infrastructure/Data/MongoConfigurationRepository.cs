using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Configuration.Infrastructure.Data;

/// <summary>
/// MongoDB implementation of IConfigurationRepository.
/// Provides CRUD operations for configuration records with service-level isolation.
/// </summary>
public sealed class MongoConfigurationRepository : IConfigurationRepository
{
    private readonly IMongoCollection<ConfigurationRecord> _collection;
    private readonly ILogger<MongoConfigurationRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the MongoConfigurationRepository.
    /// </summary>
    /// <param name="settings">Configuration settings including connection string and collection info.</param>
    /// <param name="logger">Logger instance.</param>
    public MongoConfigurationRepository(
        IOptions<ConfigurationSettings> settings,
        ILogger<MongoConfigurationRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (settings?.Value == null)
            throw new ArgumentNullException(nameof(settings));

        var mongoSettings = MongoClientSettings.FromConnectionString(settings.Value.ConnectionString);
        mongoSettings.ServerApi = new ServerApi(ServerApiVersion.V1);

        var client = new MongoClient(mongoSettings);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<ConfigurationRecord>(settings.Value.CollectionName);

        // Create index for efficient querying by ApplicationName
        var indexKeys = Builders<ConfigurationRecord>.IndexKeys.Ascending(x => x.ApplicationName);
        var indexModel = new CreateIndexModel<ConfigurationRecord>(indexKeys);
        _collection.Indexes.CreateOne(indexModel);

        _logger.LogInformation("MongoDB repository initialized for database {Database}, collection {Collection}",
            settings.Value.DatabaseName, settings.Value.CollectionName);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ConfigurationRecord>> GetByApplicationNameAsync(
        string applicationName,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ConfigurationRecord>.Filter.And(
            Builders<ConfigurationRecord>.Filter.Eq(x => x.ApplicationName, applicationName),
            Builders<ConfigurationRecord>.Filter.Eq(x => x.IsActive, 1));

        var records = await _collection.Find(filter).ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {Count} active configuration records for application {ApplicationName}",
            records.Count, applicationName);

        return records;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ConfigurationRecord>> GetAllByApplicationNameAsync(
        string applicationName,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ConfigurationRecord>.Filter.Eq(x => x.ApplicationName, applicationName);
        var records = await _collection.Find(filter).ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {Count} total configuration records for application {ApplicationName}",
            records.Count, applicationName);

        return records;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ConfigurationRecord>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var records = await _collection.Find(_ => true).ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {Count} total configuration records", records.Count);

        return records;
    }

    /// <inheritdoc />
    public async Task<ConfigurationRecord?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ConfigurationRecord>.Filter.Eq(x => x.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ConfigurationRecord> CreateAsync(
        ConfigurationRecord record,
        CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(record, cancellationToken: cancellationToken);

        _logger.LogInformation("Created configuration record {Id} for application {ApplicationName}",
            record.Id, record.ApplicationName);

        return record;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(
        ConfigurationRecord record,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ConfigurationRecord>.Filter.Eq(x => x.Id, record.Id);
        var result = await _collection.ReplaceOneAsync(filter, record, cancellationToken: cancellationToken);

        _logger.LogInformation("Updated configuration record {Id}, matched: {Matched}, modified: {Modified}",
            record.Id, result.MatchedCount, result.ModifiedCount);

        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ConfigurationRecord>.Filter.Eq(x => x.Id, id);
        var result = await _collection.DeleteOneAsync(filter, cancellationToken);

        _logger.LogInformation("Deleted configuration record {Id}, count: {Count}",
            id, result.DeletedCount);

        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}
