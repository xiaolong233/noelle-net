namespace NoelleNet.Http.Logging;

public class HttpLogEntryTests
{
    [Fact]
    public void Constructor_ShouldHaveDefaultValues()
    {
        var entry = new HttpLogEntry { CorrelationId = "test-id" };

        Assert.Equal("test-id", entry.CorrelationId);
        Assert.Equal(string.Empty, entry.Method);
        Assert.Null(entry.RequestUri);
        Assert.Equal(default, entry.Timestamp);
        Assert.Null(entry.RequestHeaders);
        Assert.Null(entry.RequestBody);
        Assert.Equal(0, entry.StatusCode);
        Assert.Null(entry.ResponseHeaders);
        Assert.Null(entry.ResponseBody);
        Assert.Equal(0, entry.DurationMs);
        Assert.Null(entry.Exception);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var entry = new HttpLogEntry
        {
            CorrelationId = "abc-123",
            Method = "POST",
            RequestUri = "https://example.com/api",
            Timestamp = new DateTime(2024, 6, 1, 12, 0, 0),
            RequestHeaders = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            RequestBody = "{\"key\":\"value\"}",
            StatusCode = 200,
            ResponseHeaders = new Dictionary<string, string> { { "Server", "nginx" } },
            ResponseBody = "{\"result\":\"ok\"}",
            DurationMs = 150,
            Exception = null
        };

        Assert.Equal("abc-123", entry.CorrelationId);
        Assert.Equal("POST", entry.Method);
        Assert.Equal("https://example.com/api", entry.RequestUri);
        Assert.Equal(200, entry.StatusCode);
        Assert.Equal(150, entry.DurationMs);
        Assert.NotNull(entry.RequestHeaders);
        Assert.NotNull(entry.ResponseHeaders);
        Assert.Single(entry.RequestHeaders);
        Assert.Single(entry.ResponseHeaders);
    }
}
