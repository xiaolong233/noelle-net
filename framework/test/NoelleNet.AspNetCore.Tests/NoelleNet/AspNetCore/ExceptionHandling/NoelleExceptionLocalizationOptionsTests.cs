namespace NoelleNet.AspNetCore.ExceptionHandling;

public class NoelleExceptionLocalizationOptionsTests
{
    [Fact]
    public void ResourceSources_Default_ShouldBeEmpty()
    {
        var options = new NoelleExceptionLocalizationOptions();

        Assert.NotNull(options.ResourceSources);
        Assert.Empty(options.ResourceSources);
    }

    [Fact]
    public void LocalizerProvider_SetAndGet_ShouldWork()
    {
        var options = new NoelleExceptionLocalizationOptions();
        options.LocalizerProvider = (ex, factory) => null!;

        Assert.NotNull(options.LocalizerProvider);
    }

    [Fact]
    public void ResourceSources_CanAddEntries()
    {
        var options = new NoelleExceptionLocalizationOptions();
        options.ResourceSources["test"] = typeof(NoelleExceptionLocalizationOptionsTests);

        Assert.Single(options.ResourceSources);
        Assert.Equal(typeof(NoelleExceptionLocalizationOptionsTests), options.ResourceSources["test"]);
    }
}
