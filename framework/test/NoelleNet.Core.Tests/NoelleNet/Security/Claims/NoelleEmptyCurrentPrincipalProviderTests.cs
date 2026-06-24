namespace NoelleNet.Security.Claims;

public class NoelleEmptyCurrentPrincipalProviderTests
{
    [Fact]
    public void Principal_ShouldReturnNewClaimsPrincipal()
    {
        var provider = new NoelleEmptyCurrentPrincipalProvider();

        Assert.NotNull(provider.Principal);
        // new ClaimsPrincipal() creates an empty principal with no identity
        Assert.NotNull(provider.Principal);
    }

    [Fact]
    public void ShouldImplementICurrentPrincipalProvider()
    {
        var provider = new NoelleEmptyCurrentPrincipalProvider();
        Assert.IsAssignableFrom<ICurrentPrincipalProvider>(provider);
    }
}
