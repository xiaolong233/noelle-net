namespace NoelleNet.Http.Security;

/// <summary>
/// <see cref="TokenManagerOptions"/> 的契约测试
/// </summary>
public class TokenManagerOptionsTests
{
    /// <summary>
    /// 过期缓冲区默认 5 分钟：令牌有效期提前 5 分钟即视为过期，是 TokenManagerBase 刷新时机的关键契约
    /// </summary>
    [Fact]
    public void ExpirationBuffer_Default_ShouldBe5Minutes()
    {
        var options = new TokenManagerOptions();

        Assert.Equal(TimeSpan.FromMinutes(5), options.ExpirationBuffer);
    }
}
