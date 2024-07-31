using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Noelle.Todo.Infrastructure;
using Noelle.Todo.WebApi.Application.IntegrationEvents;
using Noelle.Todo.WebApi.Application.Queries;
using NoelleNet.AspNetCore.Exceptions;
using NoelleNet.AspNetCore.Filters;
using NoelleNet.AspNetCore.Routing;
using NoelleNet.AspNetCore.Validation;
using NoelleNet.EntityFrameworkCore.Storage;
using NoelleNet.Extensions.MediatR;
using System.Reflection;
using System.Text;

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

        return services;
    }

    /// <summary>
    /// 添加Mvc
    /// </summary>
    /// <param name="services"></param>
    private static void AddMvc(IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            // 全局拦截器配置        
            options.Filters.Add<NoelleExceptionFilter>();
            //options.Filters.Add<NoelleModelValidationFilter>();   //基于ModelState的模型验证
            options.Filters.Add<NoelleFluentValidationFilter>();
            options.Filters.Add<NoelleResultFilter>();

            // 格式化路由：小写字母+横线
            options.Conventions.Add(new RouteTokenTransformerConvention(new NoelleRouteTokenTransformer()));

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

        // 配置授权认证
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.Jwt));
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtSettings = configuration.GetSection(JwtOptions.Jwt).Get<JwtOptions>();
                    byte[] securityKeyBuffer = Encoding.UTF8.GetBytes(jwtSettings?.SecurityKey ?? string.Empty);
                    var securityKey = new SymmetricSecurityKey(securityKeyBuffer);
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = securityKey
                    };
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

            // 定义安全方案
            var scheme = new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Description = "Authorization header \r\nExample:'Bearer ...'",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" },
            };
            options.AddSecurityDefinition("oauth2", scheme);

            // 指定安全方案的作用范围
            OpenApiSecurityRequirement requirement = new()
            {
                [scheme] = []
            };
            options.AddSecurityRequirement(requirement);
        });
    }
}
