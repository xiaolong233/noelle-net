using System.Text.Json;

namespace NoelleNet.Json.Serialization;

/// <summary>
/// <see cref="NullableDateTimeConverter"/> 的单元测试
/// </summary>
public class NullableDateTimeConverterTests
{
    /// <summary>
    /// 自定义格式应生效且使用固定文化（回归测试：曾因宿主区域设置导致输出漂移）
    /// </summary>
    [Fact]
    public void CustomFormat_ShouldUseInvariantCulture()
    {
        var converter = new NullableDateTimeConverter("yyyy/MM/dd");
        var json = JsonSerializer.Serialize<DateTime?>(new DateTime(2024, 12, 1), new JsonSerializerOptions { Converters = { converter } });

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
        Assert.ThrowsAny<ArgumentException>(() => new NullableDateTimeConverter(format!));
    }

    /// <summary>
    /// 读取 null、空串、空白或非法日期时应返回 null
    /// </summary>
    [Theory]
    [InlineData("null")]
    [InlineData("\"\"")]
    [InlineData("\"   \"")]
    [InlineData("\"invalid-date\"")]
    public void Read_InvalidInput_ShouldReturnNull(string json)
    {
        var options = new JsonSerializerOptions { Converters = { new NullableDateTimeConverter() } };

        Assert.Null(JsonSerializer.Deserialize<DateTime?>(json, options));
    }

    /// <summary>
    /// 读取合法日期应返回 DateTime
    /// </summary>
    [Fact]
    public void Read_ValidDate_ShouldReturnDateTime()
    {
        var options = new JsonSerializerOptions { Converters = { new NullableDateTimeConverter() } };

        var result = JsonSerializer.Deserialize<DateTime?>("\"2024-06-15 14:30:00\"", options);

        Assert.Equal(new DateTime(2024, 6, 15, 14, 30, 0), result);
    }

    /// <summary>
    /// 序列化：null 输出 null，有值输出格式化字符串
    /// </summary>
    [Fact]
    public void Write_ShouldOutputNullOrFormattedString()
    {
        var options = new JsonSerializerOptions { Converters = { new NullableDateTimeConverter() } };

        Assert.Equal("null", JsonSerializer.Serialize<DateTime?>(null, options));
        Assert.Equal("\"2024-01-01 00:00:00\"", JsonSerializer.Serialize<DateTime?>(new DateTime(2024, 1, 1), options));
    }
}
