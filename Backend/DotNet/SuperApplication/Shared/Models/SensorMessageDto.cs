using SuperApplication.Shared.Data.Entities.Enums;

namespace SuperApplication.Shared.Models;

public class SensorMessageDto
{
    public SensorType Type { get; set; }
    public SensorLocation Name { get; set; }
    public SensorPayloadDto? Payload { get; set; }
}