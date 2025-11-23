using SuperApplication.Shared.Data.Entities.Enums;

namespace SuperApplication.Shared.Data.Entities;

public class SensorReading
{
    public Guid Id { get; set; }
    public SensorType Type { get; set; }
    public SensorLocation Name { get; set; }
    public int? Co2 { get; set; }
    public int? Pm25 { get; set; }
    public int? Humidity { get; set; }
    public bool? MotionDetected { get; set; }
    public double? Energy { get; set; }
    public DateTime Timestamp { get; set; }
}
