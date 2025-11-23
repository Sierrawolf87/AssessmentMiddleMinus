using SuperApplication.Shared.Data.Entities;
using SuperApplication.Shared.Data.Entities.Enums;

namespace GraphQLAPI.Types;

/// <summary>
/// GraphQL object type for SensorReading entity
/// </summary>
public class SensorReadingType
{
    /// <summary>
    /// Unique identifier for the sensor reading
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Type of the sensor (e.g., AirQuality, Motion, Energy)
    /// </summary>
    public SensorType Type { get; set; }
    
    /// <summary>
    /// Name of the sensor
    /// </summary>
    public SensorLocation Name { get; set; }
    
    /// <summary>
    /// CO2 level in parts per million (ppm)
    /// </summary>
    public int? Co2 { get; set; }
    
    /// <summary>
    /// PM2.5 particulate matter level
    /// </summary>
    public int? Pm25 { get; set; }
    
    /// <summary>
    /// Humidity percentage
    /// </summary>
    public int? Humidity { get; set; }
    
    /// <summary>
    /// Motion detection status
    /// </summary>
    public bool? MotionDetected { get; set; }
    
    /// <summary>
    /// Energy consumption in kilowatt-hours (kWh)
    /// </summary>
    public double? Energy { get; set; }
    
    /// <summary>
    /// Timestamp when the reading was recorded
    /// </summary>
    public DateTime Timestamp { get; set; }
}
