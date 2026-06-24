using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.Configuration;

public class NoelleConfigurationExtensionsTests
{
    [Fact]
    public void GetRequiredValue_NullConfiguration_ShouldThrow()
    {
        IConfiguration? config = null;
        Assert.Throws<ArgumentNullException>(() => config!.GetRequiredValue("key"));
    }

    [Fact]
    public void GetRequiredValue_NullKey_ShouldThrow()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        // ArgumentException.ThrowIfNullOrWhiteSpace throws ArgumentNullException for null
        Assert.Throws<ArgumentNullException>(() => config.GetRequiredValue(null!));
    }

    [Fact]
    public void GetRequiredValue_EmptyKey_ShouldThrow()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        Assert.Throws<ArgumentException>(() => config.GetRequiredValue(""));
    }

    [Fact]
    public void GetRequiredValue_KeyExists_ShouldReturnValue()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "MyKey", "MyValue" } })
            .Build();

        var result = config.GetRequiredValue("MyKey");
        Assert.Equal("MyValue", result);
    }

    [Fact]
    public void GetRequiredValue_KeyMissing_ShouldThrowInvalidOperationException()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();

        var ex = Assert.Throws<InvalidOperationException>(() => config.GetRequiredValue("MissingKey"));
        Assert.Contains("MissingKey", ex.Message);
    }

    [Fact]
    public void GetRequiredConnectionString_NullConfiguration_ShouldThrow()
    {
        IConfiguration? config = null;
        Assert.Throws<ArgumentNullException>(() => config!.GetRequiredConnectionString("name"));
    }

    [Fact]
    public void GetRequiredConnectionString_NullName_ShouldThrow()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        Assert.Throws<ArgumentNullException>(() => config.GetRequiredConnectionString(null!));
    }

    [Fact]
    public void GetRequiredConnectionString_EmptyName_ShouldThrow()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        Assert.Throws<ArgumentException>(() => config.GetRequiredConnectionString(""));
    }

    [Fact]
    public void GetRequiredConnectionString_ConnectionExists_ShouldReturnValue()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:Default", "Server=localhost" }
            })
            .Build();

        var result = config.GetRequiredConnectionString("Default");
        Assert.Equal("Server=localhost", result);
    }

    [Fact]
    public void GetRequiredConnectionString_ConnectionMissing_ShouldThrowInvalidOperationException()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();

        var ex = Assert.Throws<InvalidOperationException>(() => config.GetRequiredConnectionString("Missing"));
        Assert.Contains("Missing", ex.Message);
    }
}
