namespace SuperApplication.Shared.Models;

public class SensorPayloadDto
{
    public int? Co2 { get; set; }
    public int? Pm25 { get; set; }
    public int? Humidity { get; set; }
    public bool? MotionDetected { get; set; }
    public double? Energy { get; set; }
}