using Configuration.Domain.DTOs;
using Configuration.Domain.Entities;

namespace Configuration.Application.Services;

/// <summary>
/// Service interface for managing configuration records.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Retrieves all configuration records across all applications.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of all configuration records.</returns>
    Task<IReadOnlyList<ConfigurationRecord>> GetAllConfigurationsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all configuration records for a specific application.
    /// </summary>
    /// <param name="applicationName">The application name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of configuration records.</returns>
    Task<IReadOnlyList<ConfigurationRecord>> GetConfigurationsByApplicationAsync(
        string applicationName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a configuration record by its ID.
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
    /// <param name="request">The creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created record.</returns>
    Task<ConfigurationRecord> CreateAsync(
        CreateConfigurationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing configuration record.
    /// </summary>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if updated; otherwise, false.</returns>
    Task<bool> UpdateAsync(
        UpdateConfigurationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a configuration record by its ID.
    /// </summary>
    /// <param name="id">The record ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);
}
