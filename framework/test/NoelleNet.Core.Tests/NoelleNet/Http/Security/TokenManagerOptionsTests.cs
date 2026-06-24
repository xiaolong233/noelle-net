namespace NoelleNet.Http.Security;

public class TokenManagerOptionsTests
{
    [Fact]
    public void DefaultExpirationBuffer_ShouldBe5Minutes()
    {
        var options = new TokenManagerOptions();
        Assert.Equal(TimeSpan.FromMinutes(5), options.ExpirationBuffer);
    }

    [Fact]
    public void ExpirationBuffer_ShouldBeSettable()
    {
        var options = new TokenManagerOptions
        {
            ExpirationBuffer = TimeSpan.FromMinutes(10)
        };

        Assert.Equal(TimeSpan.FromMinutes(10), options.ExpirationBuffer);
    }
}
