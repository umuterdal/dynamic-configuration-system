using Configuration.Domain.Entities;

namespace Configuration.Domain.Interfaces;

/// <summary>
/// Repository interface for accessing configuration records.
/// Follows Repository Pattern for data access abstraction.
/// </summary>
public interface IConfigurationRepository
{
    /// <summary>
    /// Retrieves all active configuration records for a specific application.
    /// </summary>
    /// <param name="applicationName">The application name to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of active configuration records.</returns>
    Task<IReadOnlyList<ConfigurationRecord>> GetByApplicationNameAsync(
        string applicationName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all configuration records (active and inactive) for a specific application.
    /// Used by admin panel for full list management.
    /// </summary>
    /// <param name="applicationName">The application name to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of all configuration records.</returns>
    Task<IReadOnlyList<ConfigurationRecord>> GetAllByApplicationNameAsync(
        string applicationName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all configuration records across all applications.
    /// Used by admin panel for complete list view.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of all configuration records.</returns>
    Task<IReadOnlyList<ConfigurationRecord>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific configuration record by its ID.
    /// </summary>
    /// <param name="id">The record ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The configuration record if found; otherwise, null.</returns>
    Task<ConfigurationRecord?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new configuration record.
    /// </summary>
    /// <param name="record">The record to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created record with assigned ID.</returns>
    Task<ConfigurationRecord> CreateAsync(
        ConfigurationRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing configuration record.
    /// </summary>
    /// <param name="record">The record with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if updated; otherwise, false.</returns>
    Task<bool> UpdateAsync(
        ConfigurationRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a configuration record by its ID.
    /// </summary>
    /// <param name="id">The record ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);
}
