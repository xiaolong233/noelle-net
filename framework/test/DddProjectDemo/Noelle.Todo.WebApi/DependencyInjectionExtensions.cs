using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.OpenApi.Models;
using Noelle.Todo.Infrastructure;
using Noelle.Todo.WebApi.Application.HostedServices;
using Noelle.Todo.WebApi.Application.IntegrationEvents;
using Noelle.Todo.WebApi.Application.Queries;
using NoelleNet;
using NoelleNet.AspNetCore.ExceptionHandling;
using NoelleNet.AspNetCore.Filters;
using NoelleNet.AspNetCore.Routing;
using NoelleNet.AspNetCore.Security.Claims;
using NoelleNet.AspNetCore.Validation;
using NoelleNet.EntityFrameworkCore.Storage;
using NoelleNet.Extensions.MediatR;
using NoelleNet.Security;
using NoelleNet.Security.Claims;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Validation.AspNetCore;
using Quartz;
using System.Reflection;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Noelle.Todo.WebApi;

/// <summary>
/// 依赖注入的扩展方法集
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// 添加应用服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddMvc(services);

        AddAuthentication(services, configuration);

        AddMediatR(services);

        AddIntegrationEvent(services, configuration);

        // FluentValidation配置
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());  //通过程序集加载所有Validator

        // 仓储和数据查询相关的配置
        services.AddScoped<ITodoItemQueries, TodoItemQueries>();

        // 添加健康检测
        services.AddHealthChecks();

        AddSwagger(services);

        services.AddHostedService<SeedIdentityHostedService>();

        return services;
    }

    private static readonly string[] configureOptions = ["zh-CN", "en"];
    /// <summary>
    /// 添加Mvc
    /// </summary>
    /// <param name="services"></param>
    private static void AddMvc(IServiceCollection services)
    {
        // 添加本地化
        services.AddLocalization();
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = configureOptions;
            options.SetDefaultCulture(supportedCultures[0])
                   .AddSupportedCultures(supportedCultures)
                   .AddSupportedUICultures(supportedCultures)
                   .ApplyCurrentCultureToResponseHeaders = true;
        });

        services.AddControllers(options =>
        {
            // 全局拦截器配置        
            options.Filters.Add<NoelleExceptionFilter>();
            //options.Filters.Add<NoelleModelValidationFilter>();   //基于ModelState的模型验证
            options.Filters.Add<NoelleFluentValidationFilter>();
            options.Filters.Add<NoelleActionResultStatusCodeFilter>();

            // 格式化路由：小写字母+横线
            options.Conventions.Add(new RouteTokenTransformerConvention(new NoelleRouteSnakeCaseTransformer()));

            options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
        }).ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });
        services.AddEndpointsApiExplorer();

        // 错误响应
        services.AddSingleton<IExceptionToErrorConverter, NoelleExceptionToErrorConverter>();
        services.AddSingleton<IHttpExceptionStatusCodeFinder, NoelleHttpExceptionStatusCodeFinder>();
    }

    /// <summary>
    /// 添加身份认证和授权
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ICurrentPrincipalProvider, NoelleHttpContextCurrentPrincipalProvider>();

        // 配置身份认证和授权
        services.AddIdentity<IdentityUser<long>, IdentityRole<long>>()
                .AddEntityFrameworkStores<TodoDbContext>()
                .AddDefaultTokenProviders();

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

        services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore().UseDbContext<TodoDbContext>();
                    options.UseQuartz();
                })
                .AddServer(options =>
                {
                    // 设置路由
                    options.SetAuthorizationEndpointUris("/api/auth/authorize");
                    options.SetTokenEndpointUris("/api/auth/token");
                    options.SetLogoutEndpointUris("/api/auth/logout");

                    // 设置令牌的生命周期
                    options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

                    // 设置授权模式
                    options.AllowClientCredentialsFlow()
                           .AllowPasswordFlow()
                           .AllowRefreshTokenFlow()
                           .AllowCustomFlow("quick_login");

                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                    // 添加事件处理器
                    options.AddEventHandler<OpenIddictServerEvents.HandleTokenRequestContext>(builder =>
                    {
                        builder.UseInlineHandler(async context =>
                        {
                            // 根据grant_type设置令牌的有效时长
                            if (context.Request.IsClientCredentialsGrantType())
                                context.Options.AccessTokenLifetime = TimeSpan.FromHours(2);
                            else if (context.Request.IsPasswordGrantType() || context.Request.IsRefreshTokenGrantType())
                                context.Options.AccessTokenLifetime = TimeSpan.FromMinutes(15);
                            else if (context.Request.GrantType == "quick_login")
                                context.Options.AccessTokenLifetime = TimeSpan.FromHours(1);
                            else
                                context.Options.AccessTokenLifetime = TimeSpan.FromMinutes(30);

                            // grant_type不为password、refresh_token时，禁止返回refresh_token
                            if (!context.Request.IsPasswordGrantType() && !context.Request.IsRefreshTokenGrantType())
                                context.Request.Scope = string.Join(" ", context.Request.GetScopes().Remove("offline_access"));

                            await Task.CompletedTask;
                        });
                    });

                    options.UseReferenceAccessTokens();
                    options.UseReferenceRefreshTokens();

                    // 添加签名和加密凭证
                    options.AddDevelopmentEncryptionCertificate();
                    options.AddDevelopmentSigningCertificate();

                    options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableLogoutEndpointPassthrough();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.EnableAuthorizationEntryValidation();
                    options.EnableTokenEntryValidation();
                    options.UseAspNetCore();
                });

        // 配置授权策略
        // ......
    }

    /// <summary>
    /// 添加MediatR
    /// </summary>
    /// <param name="services"></param>
    private static void AddMediatR(IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // 添加行为管道
            cfg.AddOpenBehavior(typeof(NoelleLoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(NoelleTransactionBehavior<,>));
        });
    }

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    private static void AddIntegrationEvent(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCap(options =>
        {
            options.UseEntityFramework<TodoDbContext>();
            options.UseRabbitMQ(rb =>
            {
                rb.HostName = configuration.GetRequiredValue("RabbitMQ:Host");
                rb.Port = Convert.ToInt32(configuration.GetRequiredValue("RabbitMQ:Port"));
                rb.UserName = configuration.GetRequiredValue("RabbitMQ:UserName");
                rb.Password = configuration.GetRequiredValue("RabbitMQ:Password");
            });
            options.DefaultGroupName = configuration.GetRequiredValue("RabbitMQ:GroupName");
        });
        services.AddTransient<CreateTodoItemIntegrationEventHandler>();
    }

    /// <summary>
    /// 添加Swagger
    /// </summary>
    /// <param name="services"></param>
    private static void AddSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // 设置XML注释
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), true);

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "MyAPI",
                Description = "API for my application"
            });

            string schemeName = "Bearer";
            OpenApiSecurityScheme scheme = new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = schemeName
            };

            options.AddSecurityDefinition(schemeName, scheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}
