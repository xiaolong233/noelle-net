using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Noelle.Todo.WebApi.Application.ApplicationServices;
using Noelle.Todo.WebApi.Application.HostedServices;
using Noelle.Todo.WebApi.Application.Queries;
using Noelle.Todo.WebApi.Application.Validations;
using NoelleNet.AspNetCore.ExceptionHandling;
using NoelleNet.AspNetCore.Mvc;
using NoelleNet.AspNetCore.Routing;
using NoelleNet.AspNetCore.Security.Claims;
using NoelleNet.AspNetCore.Validation;
using NoelleNet.Json.Serialization;
using NoelleNet.Security;
using NoelleNet.Security.Claims;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Validation.AspNetCore;
using Quartz;
using System.Security.Cryptography.X509Certificates;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Noelle.Todo.WebApi.Extensions;

/// <summary>
/// 依赖注入扩展方法集
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// 添加应用程序
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> 的实例</param>
    /// <param name="configuration"><see cref="IConfiguration"/> 的实例</param>
    /// <param name="env"><see cref="IWebHostEnvironment"/> 实例</param>
    /// <returns></returns>
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        AddMvc(services, env);
        AddAuthentication(services, configuration);
        services.AddModelValidations();
        services.AddQueies();
        services.AddHostedServices();
        services.AddApplicationServices();
        services.AddApplicationLocalization();
        
        return services;
    }

    /// <summary>
    /// 添加Mvc
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> 的实例</param>
    /// <param name="env"><see cref="IWebHostEnvironment"/> 实例</param>
    private static void AddMvc(IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddControllers(options =>
        {
            // 全局拦截器配置        
            options.Filters.Add<NoelleExceptionHandlingFilter>();
            //options.Filters.Add<NoelleModelValidationFilter>();   //基于ModelState的模型验证
            options.Filters.Add<NoelleFluentValidationFilter>();
            options.Filters.Add<NoelleActionResultStatusCodeFilter>();

            // 格式化路由：小写字母+中横线
            options.Conventions.Add(new RouteTokenTransformerConvention(new NoelleRouteSnakeCaseTransformer()));

            options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
        }).ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
            options.JsonSerializerOptions.Converters.Add(new NullableDateTimeConverter());
        });
        services.AddEndpointsApiExplorer();

        // 配置全局异常处理
        services.AddSingleton<IExceptionToErrorConverter, NoelleExceptionToErrorConverter>();
        services.AddSingleton<IHttpExceptionStatusCodeFinder, NoelleHttpExceptionStatusCodeFinder>();
        services.Configure<NoelleExceptionHandlingOptions>(options =>
        {
            options.IncludeExceptionDetails = env.IsDevelopment();
            options.IncludeExceptionData = env.IsDevelopment();
        });
        services.Configure<NoelleExceptionLocalizationOptions>(config =>
        {
            config.LocalizerProvider = (type, factory) =>
            {
                return factory.Create(typeof(TodoResource));
            };
        });
    }

    /// <summary>
    /// 添加身份认证和授权
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> 的实例</param>
    /// <param name="configuration"><see cref="IConfiguration"/> 的实例</param>
    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ICurrentPrincipalProvider, NoelleHttpContextCurrentPrincipalProvider>();
        NoelleClaimTypes.UserName = Claims.Name;
        NoelleClaimTypes.Role = Claims.Role;

        services.AddAuthentication(options =>
        {
            // DefaultAuthenticateScheme、DefaultChallengeScheme和DefaultSignInScheme的初始值为：Identity.Application
            // 如果不手动设置DefaultChallengeScheme，验证失败是会重定向到登录页面（Account/Login）
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        services.AddQuartz(options =>
        {
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                           .DisableBulkOperations()  //禁用EF Core使用批量操作，避免执行自动清理任务时MySQL子查询不支持使用LIMIT的问题
                           .UseDbContext<TodoDbContext>();
                    //.ReplaceDefaultEntities<Domain.Entities.Application, Authorization, Scope, Token, Guid>();
                    options.UseQuartz();
                })
                .AddServer(options =>
                {
                    // 设置路由
                    options.SetAuthorizationEndpointUris("api/connect/authorize");
                    options.SetTokenEndpointUris("api/connect/token");
                    options.SetEndSessionEndpointUris("api/connect/logout");
                    options.SetIntrospectionEndpointUris("api/connect/introspect");

                    // 设置令牌的生命周期
                    options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

                    // 设置授权模式
                    options.AllowClientCredentialsFlow()
                           .AllowPasswordFlow()
                           .AllowRefreshTokenFlow();

                    // 添加事件处理器
                    options.AddEventHandler<OpenIddictServerEvents.HandleTokenRequestContext>(builder =>
                    {
                        builder.UseInlineHandler(context =>
                        {
                            // 根据grant_type设置令牌的有效时长
                            if (context.Request.IsClientCredentialsGrantType())
                                context.Options.AccessTokenLifetime = TimeSpan.FromHours(2);
                            else if (context.Request.IsPasswordGrantType() || context.Request.IsRefreshTokenGrantType())
                                context.Options.AccessTokenLifetime = TimeSpan.FromMinutes(15);
                            else
                                context.Options.AccessTokenLifetime = TimeSpan.FromMinutes(30);

                            // grant_type不为password、refresh_token、sms时，禁止返回refresh_token
                            if (!context.Request.IsPasswordGrantType() && !context.Request.IsRefreshTokenGrantType())
                                context.Request.Scope = string.Join(" ", context.Request.GetScopes().Remove("offline_access"));

                            return ValueTask.CompletedTask;
                        });
                    });

                    options.UseReferenceAccessTokens();     //启用引用访问
                    options.UseReferenceRefreshTokens();    //启用刷新令牌

                    // 添加签名和加密凭证
                    options.AddDevelopmentEncryptionCertificate();
                    options.AddDevelopmentSigningCertificate();

                    options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough()
                           .EnableAuthorizationEndpointPassthrough()
                           .DisableTransportSecurityRequirement();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.EnableAuthorizationEntryValidation();
                    options.EnableTokenEntryValidation();
                    options.UseAspNetCore();
                });

        // 配置授权策略
        services.AddAuthorization(options =>
        {
            foreach (string permission in TodoPermissionDefinitionProvider.Permissions)
            {
                options.AddPolicy(permission, build =>
                {
                    build.RequireAssertion(ctx =>
                    {
                        if (!ctx.User.HasScope(SystemPermissions.Prefix) && !ctx.User.HasScope(permission))
                            return false;

                        if (ctx.User.IsInRole("SuperAdmin"))
                            return true;

                        if (ctx.User.HasClaim(NoelleClaimTypes.Permission, permission))
                            return true;

                        return string.IsNullOrWhiteSpace(ctx.User.Identity?.Name);
                    });
                });
            }
        });
    }
}
