using Microsoft.AspNetCore.Mvc;
using SuperApplication.Shared.Data.Entities.Enums;
using System.Text.RegularExpressions;

namespace GraphQLAPI.Controllers;

/// <summary>
/// REST API controller for sensor types
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SensorTypesController : ControllerBase
{
    /// <summary>
    /// Get all sensor types in uppercase format with underscores
    /// </summary>
    /// <returns>Array of sensor type strings</returns>
    [HttpGet]
    public ActionResult<IEnumerable<string>> GetSensorTypes()
    {
        var sensorTypes = Enum.GetValues<SensorType>()
            .Select(type =>
            {
                // Convert from PascalCase to UPPER_CASE
                var name = type.ToString();
                // Insert underscore before each uppercase letter (except the first one)
                var withUnderscores = Regex.Replace(name, "(?<!^)([A-Z])", "_$1");
                return withUnderscores.ToUpper();
            })
            .ToList();

        return Ok(sensorTypes);
    }
}
