using System.Runtime.Serialization;

namespace SuperApplication.Shared.Data.Entities.Enums;

public enum SensorLocation
{
    [EnumMember(Value = "Kitchen")]
    Kitchen,
    
    [EnumMember(Value = "Garage")]
    Garage,
    
    [EnumMember(Value = "Bedroom")]
    Bedroom,
    
    [EnumMember(Value = "Living Room")]
    LivingRoom,
    
    [EnumMember(Value = "Office")]
    Office,
    
    [EnumMember(Value = "Corridor")]
    Corridor
}
