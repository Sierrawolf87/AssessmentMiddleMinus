using System.Runtime.Serialization;

namespace SuperApplication.Shared.Data.Entities.Enums;

public enum SensorType
{
    [EnumMember(Value = "air_quality")]
    AirQuality,
    
    [EnumMember(Value = "energy")]
    Energy,
    
    [EnumMember(Value = "motion")]
    Motion
}
