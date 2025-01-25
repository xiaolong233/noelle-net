using System.Text.Json;
using System.Text.Json.Serialization;

namespace NoelleNet.Json.Serialization;

/// <summary>
/// 自定义的 <see cref="DateTime"/> 类型转换器，用于在序列化和反序列化过程中处理特定格式的日期字符串
/// </summary>
public class DateTimeConverter : JsonConverter<DateTime>
{
    private readonly string _serializationFormat;

    /// <summary>
    /// 创建一个新的 <see cref="DateTimeConverter"/> 实例
    /// </summary>
    /// <param name="serializationFormat">自定义序列化时的日期格式，默认为 "yyyy-MM-dd HH:mm:ss"</param>
    public DateTimeConverter(string serializationFormat = "yyyy-MM-dd HH:mm:ss")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serializationFormat);

        _serializationFormat = serializationFormat;
    }

    /// <summary>
    /// 反序列化方法，将 JSON 字符串转换为 <see cref="DateTime"/>
    /// </summary>
    /// <param name="reader">JSON 读取器</param>
    /// <param name="typeToConvert">目标类型，这里是 <see cref="DateTime"/></param>
    /// <param name="options">序列化选项</param>
    /// <returns></returns>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? dateString = reader.GetString();
        if (string.IsNullOrWhiteSpace(dateString))
            return DateTime.MinValue;

        if (DateTime.TryParse(dateString, out DateTime result))
            return result;

        return DateTime.MinValue;
    }

    /// <summary>
    /// 序列化方法，将 <see cref="DateTime"/> 对象转换为 JSON 字符串
    /// </summary>
    /// <param name="writer">JSON 写入器</param>
    /// <param name="value">要写入的 <see cref="DateTime"/> 对象</param>
    /// <param name="options">序列化选项</param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_serializationFormat));
    }
}
