namespace SuperApplication.Shared.Data.Entities;

public class SensorReading
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? Co2 { get; set; }
    public int? Pm25 { get; set; }
    public int? Humidity { get; set; }
    public bool? MotionDetected { get; set; }
    public double? Energy { get; set; }
    public DateTime Timestamp { get; set; }
}
