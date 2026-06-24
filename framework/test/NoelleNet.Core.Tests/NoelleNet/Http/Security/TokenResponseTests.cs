namespace NoelleNet.Http.Security;

public class TokenResponseTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var response = new TokenResponse("token123", 3600);

        Assert.Equal("token123", response.AccessToken);
        Assert.Equal(3600, response.ExpiresIn);
    }

    [Fact]
    public void Equality_ShouldWorkAsRecord()
    {
        var r1 = new TokenResponse("abc", 100);
        var r2 = new TokenResponse("abc", 100);
        var r3 = new TokenResponse("xyz", 200);

        Assert.Equal(r1, r2);
        Assert.NotEqual(r1, r3);
    }

    [Fact]
    public void Deconstruct_ShouldWork()
    {
        var response = new TokenResponse("token", 7200);
        (var token, var expiresIn) = response;

        Assert.Equal("token", token);
        Assert.Equal(7200, expiresIn);
    }
}
