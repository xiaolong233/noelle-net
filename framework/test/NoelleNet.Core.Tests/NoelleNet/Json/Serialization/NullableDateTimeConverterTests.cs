using System.Text.Json;

namespace NoelleNet.Json.Serialization;

public class NullableDateTimeConverterTests
{
    [Fact]
    public void Constructor_Default_ShouldUseDefaultFormat()
    {
        var converter = new NullableDateTimeConverter();
        var dt = new DateTime(2024, 3, 15, 10, 0, 0);
        var options = new JsonSerializerOptions { Converters = { converter } };
        var json = JsonSerializer.Serialize<DateTime?>(dt, options);
        Assert.Equal("\"2024-03-15 10:00:00\"", json);
    }

    [Fact]
    public void Constructor_NullFormat_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new NullableDateTimeConverter(null!));
    }

    [Fact]
    public void Constructor_EmptyFormat_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new NullableDateTimeConverter(""));
    }

    [Fact]
    public void Constructor_CustomFormat_ShouldUseFormat()
    {
        var converter = new NullableDateTimeConverter("yyyy/MM/dd");
        var dt = new DateTime(2024, 12, 1);
        var options = new JsonSerializerOptions { Converters = { converter } };
        var json = JsonSerializer.Serialize<DateTime?>(dt, options);
        Assert.Equal("\"2024/12/01\"", json);
    }

    [Fact]
    public void Read_NullJson_ShouldReturnNull()
    {
        var converter = new NullableDateTimeConverter();
        var options = new JsonSerializerOptions { Converters = { converter } };
        var result = JsonSerializer.Deserialize<DateTime?>("null", options);
        Assert.Null(result);
    }

    [Fact]
    public void Read_EmptyString_ShouldReturnNull()
    {
        var converter = new NullableDateTimeConverter();
        var options = new JsonSerializerOptions { Converters = { converter } };
        var result = JsonSerializer.Deserialize<DateTime?>("\"\"", options);
        Assert.Null(result);
    }

    [Fact]
    public void Read_WhitespaceString_ShouldReturnNull()
    {
        var converter = new NullableDateTimeConverter();
        var options = new JsonSerializerOptions { Converters = { converter } };
        var result = JsonSerializer.Deserialize<DateTime?>("\"   \"", options);
        Assert.Null(result);
    }

    [Fact]
    public void Read_InvalidDate_ShouldReturnNull()
    {
        var converter = new NullableDateTimeConverter();
        var options = new JsonSerializerOptions { Converters = { converter } };
        var result = JsonSerializer.Deserialize<DateTime?>("\"invalid-date\"", options);
        Assert.Null(result);
    }

    [Fact]
    public void Read_ValidDate_ShouldReturnDateTime()
    {
        var converter = new NullableDateTimeConverter();
        var options = new JsonSerializerOptions { Converters = { converter } };
        var result = JsonSerializer.Deserialize<DateTime?>("\"2024-06-15 14:30:00\"", options);
        Assert.Equal(new DateTime(2024, 6, 15, 14, 30, 0), result);
    }

    [Fact]
    public void Write_NullValue_ShouldWriteNull()
    {
        var converter = new NullableDateTimeConverter();
        var options = new JsonSerializerOptions { Converters = { converter } };
        var json = JsonSerializer.Serialize<DateTime?>(null, options);
        Assert.Equal("null", json);
    }

    [Fact]
    public void Write_ValidValue_ShouldWriteFormatted()
    {
        var converter = new NullableDateTimeConverter();
        var options = new JsonSerializerOptions { Converters = { converter } };
        var json = JsonSerializer.Serialize<DateTime?>(new DateTime(2024, 1, 1, 0, 0, 0), options);
        Assert.Equal("\"2024-01-01 00:00:00\"", json);
    }
}
