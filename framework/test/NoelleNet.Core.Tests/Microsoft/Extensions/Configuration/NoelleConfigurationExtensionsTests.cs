using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Configuration;

/// <summary>
/// <see cref="NoelleConfigurationExtensions"/> 的单元测试：强校验取值的行为契约
/// </summary>
public class NoelleConfigurationExtensionsTests
{
    /// <summary>
    /// 键存在时返回配置值
    /// </summary>
    [Fact]
    public void GetRequiredValue_KeyExists_ShouldReturnValue()
    {
        var config = Build(new Dictionary<string, string?> { { "MyKey", "MyValue" } });

        Assert.Equal("MyValue", config.GetRequiredValue("MyKey"));
    }

    /// <summary>
    /// 键缺失时抛出 InvalidOperationException，消息应包含配置键
    /// </summary>
    [Fact]
    public void GetRequiredValue_KeyMissing_ShouldThrowWithKeyInMessage()
    {
        var config = Build();

        var ex = Assert.Throws<InvalidOperationException>(() => config.GetRequiredValue("MissingKey"));

        Assert.Contains("MissingKey", ex.Message);
    }

    /// <summary>
    /// 连接字符串存在时返回其值
    /// </summary>
    [Fact]
    public void GetRequiredConnectionString_Exists_ShouldReturnValue()
    {
        var config = Build(new Dictionary<string, string?> { { "ConnectionStrings:Default", "Server=localhost" } });

        Assert.Equal("Server=localhost", config.GetRequiredConnectionString("Default"));
    }

    /// <summary>
    /// 连接字符串缺失时抛出 InvalidOperationException，消息应包含名称
    /// </summary>
    [Fact]
    public void GetRequiredConnectionString_Missing_ShouldThrowWithNameInMessage()
    {
        var config = Build();

        var ex = Assert.Throws<InvalidOperationException>(() => config.GetRequiredConnectionString("Missing"));

        Assert.Contains("Missing", ex.Message);
    }

    /// <summary>
    /// 参数为 null/空白时应抛出异常
    /// </summary>
    [Fact]
    public void InvalidArguments_ShouldThrow()
    {
        IConfiguration? nullConfig = null;
        var config = Build();

        Assert.Throws<ArgumentNullException>(() => nullConfig!.GetRequiredValue("key"));
        Assert.Throws<ArgumentNullException>(() => config.GetRequiredValue(null!));
        Assert.Throws<ArgumentException>(() => config.GetRequiredValue(""));
        Assert.Throws<ArgumentNullException>(() => config.GetRequiredConnectionString(null!));
        Assert.Throws<ArgumentException>(() => config.GetRequiredConnectionString(""));
    }

    private static IConfiguration Build(Dictionary<string, string?>? values = null)
    {
        var builder = new ConfigurationBuilder();
        if (values != null)
            builder.AddInMemoryCollection(values);
        return builder.Build();
    }
}
