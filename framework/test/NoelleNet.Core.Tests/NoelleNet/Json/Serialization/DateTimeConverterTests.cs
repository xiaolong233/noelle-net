using System.Text.Json;

namespace NoelleNet.Json.Serialization;

/// <summary>
/// <see cref="DateTimeConverter"/> 的单元测试
/// </summary>
public class DateTimeConverterTests
{
    /// <summary>
    /// 默认格式为 "yyyy-MM-dd HH:mm:ss"，往返序列化保持值不变
    /// </summary>
    [Fact]
    public void DefaultFormat_ShouldRoundTrip()
    {
        var options = new JsonSerializerOptions { Converters = { new DateTimeConverter() } };
        var value = new DateTime(2024, 6, 15, 14, 30, 0);

        var json = JsonSerializer.Serialize(value, options);
        var result = JsonSerializer.Deserialize<DateTime>(json, options);

        Assert.Equal("\"2024-06-15 14:30:00\"", json);
        Assert.Equal(value, result);
    }

    /// <summary>
    /// 自定义格式应生效，且 "/" 使用固定文化输出（回归测试：曾因宿主区域设置导致输出漂移）
    /// </summary>
    [Fact]
    public void CustomFormat_ShouldUseInvariantCulture()
    {
        var converter = new DateTimeConverter("yyyy/MM/dd");
        var json = JsonSerializer.Serialize(new DateTime(2024, 12, 1), new JsonSerializerOptions { Converters = { converter } });

        Assert.Equal("\"2024/12/01\"", json);
    }

    /// <summary>
    /// 格式为 null 或空白时应抛出异常
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidFormat_ShouldThrow(string? format)
    {
        Assert.ThrowsAny<ArgumentException>(() => new DateTimeConverter(format!));
    }

    /// <summary>
    /// 读取 null、空串、空白或非法日期时应返回 DateTime.MinValue
    /// </summary>
    [Theory]
    [InlineData("null")]
    [InlineData("\"\"")]
    [InlineData("\"   \"")]
    [InlineData("\"not-a-date\"")]
    public void Read_InvalidInput_ShouldReturnMinValue(string json)
    {
        var options = new JsonSerializerOptions { Converters = { new DateTimeConverter() } };

        var result = JsonSerializer.Deserialize<DateTime>(json, options);

        Assert.Equal(DateTime.MinValue, result);
    }

    /// <summary>
    /// MinValue 序列化应输出格式化的最小日期
    /// </summary>
    [Fact]
    public void Write_MinValue_ShouldSerializeCorrectly()
    {
        var json = JsonSerializer.Serialize(DateTime.MinValue, new JsonSerializerOptions { Converters = { new DateTimeConverter() } });

        Assert.Equal($"\"{DateTime.MinValue:yyyy-MM-dd HH:mm:ss}\"", json);
    }
}
