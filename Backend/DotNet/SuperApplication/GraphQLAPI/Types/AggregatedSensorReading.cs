namespace GraphQLAPI.Types;

/// <summary>
/// Represents sensor readings aggregated by minute intervals for efficient chart rendering
/// </summary>
public class AggregatedSensorReading
{
    /// <summary>
    /// The timestamp representing the start of the minute interval (seconds set to 0)
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Average CO2 level for all readings in this minute interval
    /// </summary>
    public double? AverageCo2 { get; set; }
    
    /// <summary>
    /// Average PM2.5 level for all readings in this minute interval
    /// </summary>
    public double? AveragePm25 { get; set; }
    
    /// <summary>
    /// Average humidity percentage for all readings in this minute interval
    /// </summary>
    public double? AverageHumidity { get; set; }
    
    /// <summary>
    /// Average energy consumption for all readings in this minute interval
    /// </summary>
    public double? AverageEnergy { get; set; }
    
    /// <summary>
    /// Total number of sensor readings aggregated in this minute interval
    /// </summary>
    public int Count { get; set; }
}
