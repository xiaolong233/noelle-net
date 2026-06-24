using System.Text.Json;

namespace NoelleNet.Json.Serialization;

public class DateTimeConverterTests
{
    private static readonly JsonSerializerOptions _options = new();

    [Fact]
    public void Constructor_Default_ShouldUseDefaultFormat()
    {
        var converter = new DateTimeConverter();
        var result = SerializeAndDeserialize(new DateTime(2024, 6, 15, 14, 30, 0), converter);

        Assert.Equal(new DateTime(2024, 6, 15, 14, 30, 0), result);
    }

    [Fact]
    public void Constructor_NullFormat_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new DateTimeConverter(null!));
    }

    [Fact]
    public void Constructor_EmptyFormat_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new DateTimeConverter(""));
    }

    [Fact]
    public void Constructor_WhitespaceFormat_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new DateTimeConverter("   "));
    }

    [Fact]
    public void Constructor_CustomFormat_ShouldUseFormat()
    {
        var converter = new DateTimeConverter("yyyy/MM/dd");
        var dt = new DateTime(2024, 12, 1);
        var json = JsonSerializer.Serialize(dt, new JsonSerializerOptions { Converters = { converter } });
        Assert.Equal("\"2024/12/01\"", json);
    }

    [Fact]
    public void Read_NullJson_ShouldReturnMinValue()
    {
        var converter = new DateTimeConverter();
        var result = JsonSerializer.Deserialize<DateTime>("null", new JsonSerializerOptions { Converters = { converter } });
        Assert.Equal(DateTime.MinValue, result);
    }

    [Fact]
    public void Read_EmptyString_ShouldReturnMinValue()
    {
        var converter = new DateTimeConverter();
        var result = JsonSerializer.Deserialize<DateTime>("\"\"", new JsonSerializerOptions { Converters = { converter } });
        Assert.Equal(DateTime.MinValue, result);
    }

    [Fact]
    public void Read_WhitespaceString_ShouldReturnMinValue()
    {
        var converter = new DateTimeConverter();
        var result = JsonSerializer.Deserialize<DateTime>("\"   \"", new JsonSerializerOptions { Converters = { converter } });
        Assert.Equal(DateTime.MinValue, result);
    }

    [Fact]
    public void Read_InvalidDate_ShouldReturnMinValue()
    {
        var converter = new DateTimeConverter();
        var result = JsonSerializer.Deserialize<DateTime>("\"not-a-date\"", new JsonSerializerOptions { Converters = { converter } });
        Assert.Equal(DateTime.MinValue, result);
    }

    [Fact]
    public void Write_MinValue_ShouldSerializeCorrectly()
    {
        var converter = new DateTimeConverter();
        var json = JsonSerializer.Serialize(DateTime.MinValue, new JsonSerializerOptions { Converters = { converter } });
        Assert.Equal($"\"{DateTime.MinValue:yyyy-MM-dd HH:mm:ss}\"", json);
    }

    private static DateTime SerializeAndDeserialize(DateTime value, DateTimeConverter converter)
    {
        var options = new JsonSerializerOptions { Converters = { converter } };
        var json = JsonSerializer.Serialize(value, options);
        return JsonSerializer.Deserialize<DateTime>(json, options);
    }
}
