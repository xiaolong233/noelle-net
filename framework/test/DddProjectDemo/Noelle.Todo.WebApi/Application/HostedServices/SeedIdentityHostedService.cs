using Microsoft.AspNetCore.Identity;
using Noelle.Todo.Infrastructure;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Noelle.Todo.WebApi.Application.HostedServices;

/// <summary>
/// 处理种子数据后台任务的托管服务
/// </summary>
public class SeedIdentityHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 创建一个新的 <see cref="SeedIdentityHostedService"/> 实例
    /// </summary>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/> 实例</param>
    public SeedIdentityHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        if (await manager.FindByClientIdAsync("admin-app", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "admin-app",
                ClientSecret = "299FB089-7CB1-4ECD-827C-11851DC74A8C",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.EndSession,
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.GrantTypes.Password,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.GrantTypes.AuthorizationCode,
                    "gt:quick_login",
                    Permissions.ResponseTypes.Code,
                }
            }, cancellationToken);
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<long>>>();
        var user = await userManager.FindByNameAsync("user@example.com");
        if (user == null)
        {
            user = new IdentityUser<long>("user@example.com")
            {
                Email = "user@example.com"
            };
            var result = await userManager.CreateAsync(user, "Qazwsx@123456");
        }
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
