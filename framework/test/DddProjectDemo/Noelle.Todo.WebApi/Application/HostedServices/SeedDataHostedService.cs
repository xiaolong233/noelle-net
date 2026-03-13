using NoelleNet.Uow;
using OpenIddict.Abstractions;

namespace Noelle.Todo.WebApi.Application.HostedServices;

/// <summary>
/// 处理种子数据后台任务的托管服务
/// </summary>
public class SeedDataHostedService : IHostedService
{
    private const string clientId = "22feca683af540888a47bf2564c324e0";
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 创建一个新的 <see cref="SeedDataHostedService"/> 实例
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    /// <exception cref="ArgumentNullException"></exception>
    public SeedDataHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        ILogger<SeedDataHostedService> logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedDataHostedService>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        #region 初始化授权范围
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        if (await scopeManager.FindByNameAsync(OpenIddictConstants.Scopes.Email, cancellationToken) == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = OpenIddictConstants.Scopes.Email,
                DisplayName = "邮箱地址",
                Description = "请求访问用户的电子邮件地址",
                Resources = { clientId }
            }, cancellationToken);
        }

        if (await scopeManager.FindByNameAsync(OpenIddictConstants.Scopes.Phone, cancellationToken) == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = OpenIddictConstants.Scopes.Phone,
                DisplayName = "电话号码",
                Description = "请求访问用户的电话号码",
                Resources = { clientId }
            }, cancellationToken);
        }

        if (await scopeManager.FindByNameAsync(OpenIddictConstants.Scopes.Address, cancellationToken) == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = OpenIddictConstants.Scopes.Address,
                DisplayName = "地址信息",
                Description = "请求访问用户的地址信息",
                Resources = { clientId }
            }, cancellationToken);
        }

        if (await scopeManager.FindByNameAsync(OpenIddictConstants.Scopes.Roles, cancellationToken) == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = OpenIddictConstants.Scopes.Roles,
                DisplayName = "角色",
                Description = "请求访问用户的角色信息",
                Resources = { clientId }
            }, cancellationToken);
        }

        if (await scopeManager.FindByNameAsync(OpenIddictConstants.Scopes.Profile, cancellationToken) == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = OpenIddictConstants.Scopes.Profile,
                DisplayName = "基本个人信息",
                Description = "请求访问用户的基本个人信息（如姓名、头像等）",
                Resources = { clientId }
            }, cancellationToken);
        }

        var scopeName = "todo";
        if (await scopeManager.FindByNameAsync(scopeName, cancellationToken) == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = scopeName,
                DisplayName = "待办事项的API",
                Description = "请求访问待办事项服务的所有资源",
                Resources = { clientId }
            }, cancellationToken);
        }
        #endregion

        #region 初始化应用程序
        var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // 身份认证和授权服务
        if ((await appManager.FindByClientIdAsync(clientId, cancellationToken)) == null)
        {
            OpenIddictApplicationDescriptor descriptor = new()
            {
                ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                DisplayName = "身份与访问管理服务",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                ClientId = clientId,
                ClientSecret = "903b4304195a4aaf948db422a55a4e29",
                Permissions = {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Phone,
                    OpenIddictConstants.Permissions.Scopes.Address,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    $"{OpenIddictConstants.Permissions.Prefixes.Scope}{scopeName}"
                },
            };

            await appManager.CreateAsync(descriptor, cancellationToken);
        }
        #endregion
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}