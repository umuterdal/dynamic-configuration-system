using Configuration.Library;
using Microsoft.AspNetCore.Mvc;

namespace Configuration.DemoApi.Controllers;

/// <summary>
/// Demonstrates how to use the ConfigurationReader library.
/// Provides endpoints to retrieve, type-cast, list, and refresh dynamic configurations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DemoController : ControllerBase
{
    private readonly ConfigurationReader _configurationReader;
    private readonly ILogger<DemoController> _logger;

    /// <summary>
    /// Initializes a new instance of the DemoController.
    /// </summary>
    /// <param name="configurationReader">The configuration reader with caching and automatic refresh.</param>
    /// <param name="logger">Logger instance.</param>
    public DemoController(
        ConfigurationReader configurationReader,
        ILogger<DemoController> logger)
    {
        _configurationReader = configurationReader ?? throw new ArgumentNullException(nameof(configurationReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a configuration value by key as a string.
    /// </summary>
    /// <param name="key">The configuration key (e.g., "SiteName", "TaxRate").</param>
    /// <returns>The configuration value as a string.</returns>
    /// <response code="200">Returns the configuration value.</response>
    /// <response code="404">Configuration with the specified key was not found.</response>
    [HttpGet("value/{key}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public IActionResult GetValue(string key)
    {
        try
        {
            var value = _configurationReader.GetValue<string>(key);
            _logger.LogInformation("Retrieved configuration value for key: {Key}", key);
            return Ok(new { Key = key, Value = value });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Configuration key not found: {Key}", key);
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a typed configuration value by key.
    /// Supports string, int, double, and bool types.
    /// </summary>
    /// <param name="key">The configuration key (e.g., "MaxItemCount", "TaxRate", "MaintenanceMode").</param>
    /// <param name="type">The expected type: "string", "int", "double", or "bool". Default is "string".</param>
    /// <returns>The configuration value cast to the requested type.</returns>
    /// <response code="200">Returns the typed configuration value.</response>
    /// <response code="400">Type conversion failed (e.g., value is not compatible with the requested type).</response>
    /// <response code="404">Configuration with the specified key was not found.</response>
    [HttpGet("typed/{key}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public IActionResult GetTypedValue(string key, [FromQuery] string type = "string")
    {
        try
        {
            object value = type.ToLowerInvariant() switch
            {
                "int" => _configurationReader.GetValue<int>(key),
                "double" => _configurationReader.GetValue<double>(key),
                "bool" => _configurationReader.GetValue<bool>(key),
                _ => _configurationReader.GetValue<string>(key)
            };

            _logger.LogInformation("Retrieved typed configuration: {Key} as {Type}", key, type);
            return Ok(new { Key = key, Type = type, Value = value });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Configuration key not found: {Key}", key);
            return NotFound(new { Error = ex.Message });
        }
        catch (InvalidCastException ex)
        {
            _logger.LogWarning("Type conversion failed for key: {Key}, type: {Type}", key, type);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all active configuration values for the configured application.
    /// Returns a dictionary where keys are configuration names and values are their string representations.
    /// </summary>
    /// <returns>Dictionary of all active configuration key-value pairs.</returns>
    /// <response code="200">Returns all active configurations.</response>
    [HttpGet("all")]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public IActionResult GetAllValues()
    {
        var values = _configurationReader.GetAllValues();
        _logger.LogInformation("Retrieved all configurations. Count: {Count}", values.Count);
        return Ok(values);
    }

    /// <summary>
    /// Forces an immediate refresh of the configuration cache from MongoDB.
    /// Useful when configurations are updated and you need the latest values without waiting for the next polling interval.
    /// </summary>
    /// <returns>Success message confirming the refresh completed.</returns>
    /// <response code="200">Cache refreshed successfully.</response>
    [HttpGet("refresh")]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh()
    {
        await _configurationReader.RefreshAsync();
        _logger.LogInformation("Configuration cache refreshed manually");
        return Ok(new { Message = "Configuration refreshed successfully" });
    }
}
