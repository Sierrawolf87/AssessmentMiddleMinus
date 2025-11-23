using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperApplication.Shared.Data.JsonConverters;

public class EnumMemberConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    private readonly Dictionary<TEnum, string> _enumToString = new();
    private readonly Dictionary<string, TEnum> _stringToEnum = new();

    public EnumMemberConverter()
    {
        var type = typeof(TEnum);
        var values = Enum.GetValues<TEnum>();

        foreach (var value in values)
        {
            var enumMember = type.GetMember(value.ToString())[0];
            var attr = enumMember.GetCustomAttribute<EnumMemberAttribute>();
            var stringValue = attr?.Value ?? value.ToString();
            
            _enumToString[value] = stringValue;
            _stringToEnum[stringValue] = value;
        }
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        
        if (stringValue != null && _stringToEnum.TryGetValue(stringValue, out var enumValue))
        {
            return enumValue;
        }

        throw new JsonException($"Unable to convert \"{stringValue}\" to enum \"{typeof(TEnum)}\"");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(Convert.ToInt32(value));
    }
}
