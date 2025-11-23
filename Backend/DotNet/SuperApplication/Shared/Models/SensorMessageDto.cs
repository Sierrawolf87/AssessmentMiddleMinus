namespace SuperApplication.Shared.Models;

public class SensorMessageDto
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public SensorPayloadDto? Payload { get; set; }
}