namespace GraphQLAPI.Types;

/// <summary>
/// Statistics and aggregation data for sensor readings
/// </summary>
public class SensorReadingStats
{
    /// <summary>
    /// Total number of sensor readings
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Average CO2 level across all readings with CO2 data
    /// </summary>
    public double? AverageCo2 { get; set; }
    
    /// <summary>
    /// Average PM2.5 level across all readings with PM2.5 data
    /// </summary>
    public double? AveragePm25 { get; set; }
    
    /// <summary>
    /// Average humidity across all readings with humidity data
    /// </summary>
    public double? AverageHumidity { get; set; }
    
    /// <summary>
    /// Average energy consumption across all readings with energy data
    /// </summary>
    public double? AverageEnergy { get; set; }
    
    /// <summary>
    /// Maximum CO2 level recorded
    /// </summary>
    public int? MaxCo2 { get; set; }
    
    /// <summary>
    /// Minimum CO2 level recorded
    /// </summary>
    public int? MinCo2 { get; set; }
    
    /// <summary>
    /// Maximum PM2.5 level recorded
    /// </summary>
    public int? MaxPm25 { get; set; }
    
    /// <summary>
    /// Minimum PM2.5 level recorded
    /// </summary>
    public int? MinPm25 { get; set; }
    
    /// <summary>
    /// Number of readings with motion detected
    /// </summary>
    public int MotionDetectedCount { get; set; }
}
