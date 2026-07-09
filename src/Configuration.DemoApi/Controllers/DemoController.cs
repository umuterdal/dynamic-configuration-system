using Configuration.Library;
using Microsoft.AspNetCore.Mvc;

namespace Configuration.DemoApi.Controllers;

/// <summary>
/// Demo controller that demonstrates ConfigurationReader usage.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    private readonly ConfigurationReader _configurationReader;
    private readonly ILogger<DemoController> _logger;

    /// <summary>
    /// Initializes a new instance of the DemoController.
    /// </summary>
    /// <param name="configurationReader">The configuration reader.</param>
    /// <param name="logger">Logger instance.</param>
    public DemoController(
        ConfigurationReader configurationReader,
        ILogger<DemoController> logger)
    {
        _configurationReader = configurationReader ?? throw new ArgumentNullException(nameof(configurationReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a configuration value by key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value.</returns>
    [HttpGet("value/{key}")]
    public IActionResult GetValue(string key)
    {
        try
        {
            var value = _configurationReader.GetValue<string>(key);
            return Ok(new { Key = key, Value = value });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a typed configuration value by key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="type">The expected type (string, int, double, bool).</param>
    /// <returns>The typed configuration value.</returns>
    [HttpGet("typed/{key}")]
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

            return Ok(new { Key = key, Type = type, Value = value });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (InvalidCastException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all active configuration values.
    /// </summary>
    /// <returns>All configuration values as dictionary.</returns>
    [HttpGet("all")]
    public IActionResult GetAllValues()
    {
        var values = _configurationReader.GetAllValues();
        return Ok(values);
    }

    /// <summary>
    /// Forces a refresh of the configuration cache.
    /// </summary>
    /// <returns>Success status.</returns>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        await _configurationReader.RefreshAsync();
        return Ok(new { Message = "Configuration refreshed successfully" });
    }
}
