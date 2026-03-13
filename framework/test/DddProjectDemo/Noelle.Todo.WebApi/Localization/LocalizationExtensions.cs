namespace Noelle.Todo.WebApi.Localization;

/// <summary>
/// 本地化扩展方法集
/// </summary>
public static class LocalizationExtensions
{
    private static readonly string[] _cultures = ["zh-CN", "en"];

    /// <summary>
    /// 添加应用程序的本地化配置
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddApplicationLocalization(this IServiceCollection services)
    {
        services.AddLocalization();

        // 请求本地化选项
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = _cultures;
            options.SetDefaultCulture(supportedCultures[0])
                   .AddSupportedCultures(supportedCultures)
                   .AddSupportedUICultures(supportedCultures)
                   .ApplyCurrentCultureToResponseHeaders = true;
        });

        return services;
    }
}