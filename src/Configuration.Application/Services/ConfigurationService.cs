using Configuration.Domain.DTOs;
using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Configuration.Application.Services;

/// <summary>
/// Service for managing configuration records.
/// Handles business logic for CRUD operations.
/// </summary>
public sealed class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _repository;
    private readonly ILogger<ConfigurationService> _logger;

    /// <summary>
    /// Initializes a new instance of the ConfigurationService.
    /// </summary>
    /// <param name="repository">The configuration repository.</param>
    /// <param name="logger">Logger instance.</param>
    public ConfigurationService(
        IConfigurationRepository repository,
        ILogger<ConfigurationService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ConfigurationRecord>> GetAllConfigurationsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving all configurations");
        return await _repository.GetAllAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ConfigurationRecord>> GetConfigurationsByApplicationAsync(
        string applicationName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
            throw new ArgumentException("Application name cannot be null or empty.", nameof(applicationName));

        _logger.LogDebug("Retrieving configurations for application {ApplicationName}", applicationName);
        return await _repository.GetAllByApplicationNameAsync(applicationName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ConfigurationRecord?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be null or empty.", nameof(id));

        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ConfigurationRecord> CreateAsync(
        CreateConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        var record = new ConfigurationRecord
        {
            Name = request.Name,
            Type = request.Type,
            Value = request.Value,
            IsActive = request.IsActive,
            ApplicationName = request.ApplicationName
        };

        _logger.LogInformation("Creating configuration {Name} for application {ApplicationName}",
            request.Name, request.ApplicationName);

        return await _repository.CreateAsync(record, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(
        UpdateConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Id))
            throw new ArgumentException("ID cannot be null or empty.", nameof(request.Id));

        ValidateRequest(request);

        var record = new ConfigurationRecord
        {
            Id = request.Id,
            Name = request.Name,
            Type = request.Type,
            Value = request.Value,
            IsActive = request.IsActive,
            ApplicationName = request.ApplicationName
        };

        _logger.LogInformation("Updating configuration {Id}", request.Id);

        return await _repository.UpdateAsync(record, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be null or empty.", nameof(id));

        _logger.LogInformation("Deleting configuration {Id}", id);

        return await _repository.DeleteAsync(id, cancellationToken);
    }

    private static void ValidateRequest(CreateConfigurationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Type))
            throw new ArgumentException("Type cannot be null or empty.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Value))
            throw new ArgumentException("Value cannot be null or empty.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.ApplicationName))
            throw new ArgumentException("ApplicationName cannot be null or empty.", nameof(request));

        var validTypes = new[] { "string", "int", "double", "bool" };
        if (!validTypes.Contains(request.Type.ToLowerInvariant()))
            throw new ArgumentException($"Type must be one of: {string.Join(", ", validTypes)}", nameof(request));
    }

    private static void ValidateRequest(UpdateConfigurationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Type))
            throw new ArgumentException("Type cannot be null or empty.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Value))
            throw new ArgumentException("Value cannot be null or empty.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.ApplicationName))
            throw new ArgumentException("ApplicationName cannot be null or empty.", nameof(request));

        var validTypes = new[] { "string", "int", "double", "bool" };
        if (!validTypes.Contains(request.Type.ToLowerInvariant()))
            throw new ArgumentException($"Type must be one of: {string.Join(", ", validTypes)}", nameof(request));
    }
}
