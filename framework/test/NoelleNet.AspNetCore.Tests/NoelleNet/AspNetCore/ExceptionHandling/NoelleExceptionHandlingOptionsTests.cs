namespace NoelleNet.AspNetCore.ExceptionHandling;

public class NoelleExceptionHandlingOptionsTests
{
    [Fact]
    public void Default_Properties_ShouldHaveCorrectDefaults()
    {
        var options = new NoelleExceptionHandlingOptions();

        Assert.Null(options.TraceIdProvider);
        Assert.False(options.IncludeExceptionDetails);
        Assert.False(options.IncludeStackTrace);
        Assert.False(options.IncludeExceptionData);
    }

    [Fact]
    public void TraceIdProvider_SetAndGet_ShouldWork()
    {
        var options = new NoelleExceptionHandlingOptions();
        options.TraceIdProvider = () => "trace-123";

        Assert.NotNull(options.TraceIdProvider);
        Assert.Equal("trace-123", options.TraceIdProvider());
    }

    [Fact]
    public void IncludeExceptionDetails_SetToTrue_ShouldBeTrue()
    {
        var options = new NoelleExceptionHandlingOptions();
        options.IncludeExceptionDetails = true;

        Assert.True(options.IncludeExceptionDetails);
    }

    [Fact]
    public void IncludeStackTrace_SetToTrue_ShouldBeTrue()
    {
        var options = new NoelleExceptionHandlingOptions();
        options.IncludeStackTrace = true;

        Assert.True(options.IncludeStackTrace);
    }

    [Fact]
    public void IncludeExceptionData_SetToTrue_ShouldBeTrue()
    {
        var options = new NoelleExceptionHandlingOptions();
        options.IncludeExceptionData = true;

        Assert.True(options.IncludeExceptionData);
    }
}
