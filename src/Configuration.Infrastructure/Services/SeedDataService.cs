using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Configuration.Infrastructure.Services;

/// <summary>
/// Background service that seeds initial configuration data on startup.
/// Only seeds if the database is empty (idempotent).
/// </summary>
public sealed class SeedDataService : BackgroundService
{
    private readonly IConfigurationRepository _repository;
    private readonly ILogger<SeedDataService> _logger;

    public SeedDataService(
        IConfigurationRepository repository,
        ILogger<SeedDataService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var existing = await _repository.GetAllAsync(stoppingToken);
            if (existing.Count > 0)
            {
                _logger.LogInformation("Database already contains {Count} records, skipping seed", existing.Count);
                return;
            }

            _logger.LogInformation("Seeding initial configuration data...");

            var seedRecords = GetSeedRecords();

            foreach (var record in seedRecords)
            {
                await _repository.CreateAsync(record, stoppingToken);
            }

            _logger.LogInformation("Successfully seeded {Count} configuration records", seedRecords.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed configuration data");
        }
    }

    private static List<ConfigurationRecord> GetSeedRecords()
    {
        return new List<ConfigurationRecord>
        {
            // SERVICE-A configurations
            new()
            {
                Name = "SiteName",
                Type = "string",
                Value = "soty.io",
                IsActive = 1,
                ApplicationName = "SERVICE-A"
            },
            new()
            {
                Name = "MaxItemCount",
                Type = "int",
                Value = "500",
                IsActive = 1,
                ApplicationName = "SERVICE-A"
            },
            new()
            {
                Name = "TaxRate",
                Type = "double",
                Value = "0.18",
                IsActive = 1,
                ApplicationName = "SERVICE-A"
            },
            new()
            {
                Name = "IsBasketEnabled",
                Type = "bool",
                Value = "true",
                IsActive = 1,
                ApplicationName = "SERVICE-A"
            },
            new()
            {
                Name = "MaintenanceMode",
                Type = "bool",
                Value = "false",
                IsActive = 0,
                ApplicationName = "SERVICE-A"
            },

            // SERVICE-B configurations
            new()
            {
                Name = "IsBasketEnabled",
                Type = "bool",
                Value = "true",
                IsActive = 1,
                ApplicationName = "SERVICE-B"
            },
            new()
            {
                Name = "MaxOrderCount",
                Type = "int",
                Value = "100",
                IsActive = 1,
                ApplicationName = "SERVICE-B"
            },
            new()
            {
                Name = "Currency",
                Type = "string",
                Value = "TRY",
                IsActive = 1,
                ApplicationName = "SERVICE-B"
            },
            new()
            {
                Name = "PaymentGateway",
                Type = "string",
                Value = "iyzico",
                IsActive = 1,
                ApplicationName = "SERVICE-B"
            }
        };
    }
}
