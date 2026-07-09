using Configuration.Application.Services;
using Configuration.Domain.DTOs;
using Configuration.Domain.Interfaces;
using Configuration.Library;
using Microsoft.AspNetCore.Mvc;

namespace Configuration.Admin.Controllers;

/// <summary>
/// Controller for managing configuration records via web UI.
/// Publishes change events to RabbitMQ for instant consumer refresh.
/// </summary>
public class ConfigurationController : Controller
{
    private readonly IConfigurationService _configurationService;
    private readonly IConfigurationBrokerPublisher? _brokerPublisher;
    private readonly ConfigurationReader? _configurationReader;
    private readonly ILogger<ConfigurationController> _logger;

    /// <summary>
    /// Initializes a new instance of the ConfigurationController.
    /// </summary>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="brokerPublisher">Optional broker publisher for change events.</param>
    /// <param name="configurationReader">Optional configuration reader for health checks.</param>
    /// <param name="logger">Logger instance.</param>
    public ConfigurationController(
        IConfigurationService configurationService,
        ILogger<ConfigurationController> logger,
        IConfigurationBrokerPublisher? brokerPublisher = null,
        ConfigurationReader? configurationReader = null)
    {
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _brokerPublisher = brokerPublisher;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configurationReader = configurationReader;
    }

    /// <summary>
    /// Displays all configuration records.
    /// </summary>
    public async Task<IActionResult> Index(string? applicationName, CancellationToken cancellationToken)
    {
        var configurations = string.IsNullOrWhiteSpace(applicationName)
            ? await _configurationService.GetAllConfigurationsAsync(cancellationToken)
            : await _configurationService.GetConfigurationsByApplicationAsync(applicationName, cancellationToken);

        ViewBag.ApplicationName = applicationName;
        return View(configurations);
    }

    /// <summary>
    /// Displays the create form.
    /// </summary>
    public IActionResult Create()
    {
        return View(new CreateConfigurationRequest { IsActive = 1 });
    }

    /// <summary>
    /// Creates a new configuration record.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        try
        {
            await _configurationService.CreateAsync(request, cancellationToken);

            if (_brokerPublisher != null)
                await _brokerPublisher.PublishAsync(request.ApplicationName, "Created", cancellationToken);

            TempData["SuccessMessage"] = "Configuration created successfully.";
            return RedirectToAction(nameof(Index), new { applicationName = request.ApplicationName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating configuration");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the configuration.");
            return View(request);
        }
    }

    /// <summary>
    /// Displays the edit form for a configuration record.
    /// </summary>
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var record = await _configurationService.GetByIdAsync(id, cancellationToken);
        if (record == null)
            return NotFound();

        var request = new UpdateConfigurationRequest
        {
            Id = record.Id,
            Name = record.Name,
            Type = record.Type,
            Value = record.Value,
            IsActive = record.IsActive,
            ApplicationName = record.ApplicationName
        };

        return View(request);
    }

    /// <summary>
    /// Updates an existing configuration record.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        UpdateConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        try
        {
            await _configurationService.UpdateAsync(request, cancellationToken);

            if (_brokerPublisher != null)
                await _brokerPublisher.PublishAsync(request.ApplicationName, "Updated", cancellationToken);

            TempData["SuccessMessage"] = "Configuration updated successfully.";
            return RedirectToAction(nameof(Index), new { applicationName = request.ApplicationName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration {Id}", request.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the configuration.");
            return View(request);
        }
    }

    /// <summary>
    /// Confirms deletion of a configuration record.
    /// </summary>
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var record = await _configurationService.GetByIdAsync(id, cancellationToken);
        if (record == null)
            return NotFound();

        return View(record);
    }

    /// <summary>
    /// Deletes a configuration record.
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id, CancellationToken cancellationToken)
    {
        try
        {
            var record = await _configurationService.GetByIdAsync(id, cancellationToken);
            await _configurationService.DeleteAsync(id, cancellationToken);

            if (_brokerPublisher != null && record != null)
                await _brokerPublisher.PublishAsync(record.ApplicationName, "Deleted", cancellationToken);

            TempData["SuccessMessage"] = "Configuration deleted successfully.";
            return RedirectToAction(nameof(Index), new { applicationName = record?.ApplicationName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting configuration {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the configuration.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Gets system health status for the admin panel footer.
    /// Returns MongoDB connectivity status.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Health()
    {
        try
        {
            if (_configurationReader != null)
            {
                var healthInfo = await _configurationReader.GetHealthInfoAsync();
                return Json(healthInfo);
            }
            return Json(new { MongoDB = "not_configured" });
        }
        catch
        {
            return Json(new { MongoDB = "unhealthy" });
        }
    }

    /// <summary>
    /// Returns configurations as JSON for auto-refresh polling.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConfigurations(string? applicationName, CancellationToken cancellationToken)
    {
        var configurations = string.IsNullOrWhiteSpace(applicationName)
            ? await _configurationService.GetAllConfigurationsAsync(cancellationToken)
            : await _configurationService.GetConfigurationsByApplicationAsync(applicationName, cancellationToken);

        return Json(configurations.Select(c => new
        {
            c.Id,
            c.Name,
            c.Type,
            c.Value,
            c.IsActive,
            c.ApplicationName
        }));
    }
}
