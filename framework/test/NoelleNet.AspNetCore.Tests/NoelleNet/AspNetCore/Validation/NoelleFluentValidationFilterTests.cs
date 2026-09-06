using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using NoelleNet.AspNetCore.Validation.Localization;
using NoelleNet.Validation;

namespace NoelleNet.AspNetCore.Validation;

/// <summary>
/// <see cref="NoelleFluentValidationFilter"/> 的行为测试：
/// 按参数类型解析验证器、验证失败抛 NoelleValidationException、跳过不参与验证的参数
/// </summary>
public class NoelleFluentValidationFilterTests
{
    private static NoelleFluentValidationFilter CreateFilter()
    {
        var localizer = new Mock<IStringLocalizer<NoelleValidationResource>>();
        localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns(new LocalizedString("ParameterRequiredErrorMessage", "The parameter is required."));
        return new NoelleFluentValidationFilter(localizer.Object);
    }

    private static ActionExecutingContext CreateContext(object? argument, ParameterDescriptor parameter, ServiceProvider serviceProvider)
    {
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor { Parameters = [parameter] });
        return new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?> { [parameter.Name] = argument },
            controller: null!);
    }

    private static ParameterDescriptor CreateParameter(Type parameterType, string name = "model")
    {
        return new ParameterDescriptor { Name = name, ParameterType = parameterType };
    }

    private static ServiceProvider BuildProvider(params (Type Service, Type Implementation)[] registrations)
    {
        var services = new ServiceCollection();
        foreach (var (service, implementation) in registrations)
            services.AddScoped(service, implementation);
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// 模型有效时应继续执行后续管道
    /// </summary>
    [Fact]
    public async Task OnActionExecutionAsync_ValidModel_ShouldInvokeNext()
    {
        var filter = CreateFilter();
        var context = CreateContext(new TestModel { Name = "Noelle" }, CreateParameter(typeof(TestModel)),
            BuildProvider((typeof(IValidator<TestModel>), typeof(TestModelValidator))));
        bool nextInvoked = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ActionExecutedContext(context, [], controller: null!));
        });

        Assert.True(nextInvoked);
    }

    /// <summary>
    /// 模型无效时应抛 NoelleValidationException，携带错误消息与成员名；多验证器收集全部错误
    /// </summary>
    [Fact]
    public async Task OnActionExecutionAsync_InvalidModel_ShouldThrowValidationException()
    {
        var filter = CreateFilter();
        var context = CreateContext(new TestModel { Name = string.Empty }, CreateParameter(typeof(TestModel)),
            BuildProvider(
                (typeof(IValidator<TestModel>), typeof(TestModelValidator)),
                (typeof(IValidator<TestModel>), typeof(TestModelMinLengthValidator))));

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(context, [], controller: null!))));

        Assert.Equal(2, exception.ValidationResults.Count());
        Assert.Contains(exception.ValidationResults, r => r.ErrorMessage == "Name is required");
        Assert.Contains(exception.ValidationResults, r => r.MemberNames.Contains("Name"));
    }

    /// <summary>
    /// 参数为 null 且未声明 EmptyBodyBehavior.Allow 时应报参数必填错误
    /// </summary>
    [Fact]
    public async Task OnActionExecutionAsync_NullArgument_ShouldThrowParameterRequired()
    {
        var filter = CreateFilter();
        var context = CreateContext(null, CreateParameter(typeof(TestModel)),
            BuildProvider((typeof(IValidator<TestModel>), typeof(TestModelValidator))));

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(context, [], controller: null!))));

        var result = Assert.Single(exception.ValidationResults);
        Assert.Equal("The parameter is required.", result.ErrorMessage);
        Assert.Contains("model", result.MemberNames);
    }

    /// <summary>
    /// 应跳过验证的参数：CancellationToken、[FromServices]/Special 绑定源、无验证器的类型、EmptyBodyBehavior.Allow 的空参数
    /// </summary>
    [Fact]
    public async Task OnActionExecutionAsync_SkippableParameters_ShouldInvokeNext()
    {
        var filter = CreateFilter();
        var provider = BuildProvider((typeof(IValidator<TestModel>), typeof(TestModelValidator)));

        var cancellationTokenParameter = CreateParameter(typeof(CancellationToken));
        var fromServicesParameter = CreateParameter(typeof(TestModel));
        fromServicesParameter.BindingInfo = new BindingInfo { BindingSource = BindingSource.Services };
        var specialParameter = CreateParameter(typeof(TestModel));
        specialParameter.BindingInfo = new BindingInfo { BindingSource = BindingSource.Special };
        var allowEmptyParameter = CreateParameter(typeof(TestModel));
        allowEmptyParameter.BindingInfo = new BindingInfo { EmptyBodyBehavior = EmptyBodyBehavior.Allow };

        foreach (var (argument, parameter) in new (object?, ParameterDescriptor)[]
        {
            (null, cancellationTokenParameter),
            (null, fromServicesParameter),
            (null, specialParameter),
            ("no-validator", CreateParameter(typeof(string))),
            (null, allowEmptyParameter)
        })
        {
            var context = CreateContext(argument, parameter, provider);
            bool nextInvoked = false;
            await filter.OnActionExecutionAsync(context, () =>
            {
                nextInvoked = true;
                return Task.FromResult(new ActionExecutedContext(context, [], controller: null!));
            });
            Assert.True(nextInvoked);
        }
    }

    /// <summary>
    /// 模型级错误应携带空成员名（映射到 ProblemDetails 的 errors[""]）
    /// </summary>
    [Fact]
    public async Task OnActionExecutionAsync_ModelLevelError_ShouldContainEmptyMemberName()
    {
        var filter = CreateFilter();
        var context = CreateContext(new TestModel { Name = "Noelle" }, CreateParameter(typeof(TestModel)),
            BuildProvider((typeof(IValidator<TestModel>), typeof(TestModelLevelValidator))));

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(context, [], controller: null!))));

        var result = Assert.Single(exception.ValidationResults);
        Assert.Equal(string.Empty, Assert.Single(result.MemberNames));
    }

    /// <summary>
    /// 派生过滤器重写 GetMemberName 后应使用自定义成员名
    /// </summary>
    [Fact]
    public async Task OnActionExecutionAsync_OverriddenGetMemberName_ShouldUseCustomMemberName()
    {
        var localizer = new Mock<IStringLocalizer<NoelleValidationResource>>();
        var filter = new CustomMemberNameFilter(localizer.Object);
        var context = CreateContext(new TestModel { Name = null }, CreateParameter(typeof(TestModel)),
            BuildProvider((typeof(IValidator<TestModel>), typeof(TestModelValidator))));

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(context, [], controller: null!))));

        var result = Assert.Single(exception.ValidationResults);
        Assert.Equal("custom.Name", Assert.Single(result.MemberNames));
    }

    public class TestModel
    {
        public string? Name { get; set; }
    }

    public class TestModelValidator : AbstractValidator<TestModel>
    {
        public TestModelValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        }
    }

    public class TestModelMinLengthValidator : AbstractValidator<TestModel>
    {
        public TestModelMinLengthValidator()
        {
            RuleFor(x => x.Name).MinimumLength(3).WithMessage("Name must be at least 3 characters");
        }
    }

    public class TestModelLevelValidator : AbstractValidator<TestModel>
    {
        public TestModelLevelValidator()
        {
            RuleFor(x => x).Must(_ => false).WithMessage("Model is invalid");
        }
    }

    public class CustomMemberNameFilter : NoelleFluentValidationFilter
    {
        public CustomMemberNameFilter(IStringLocalizer<NoelleValidationResource> localizer) : base(localizer)
        {
        }

        protected override string GetMemberName(Type parameterType, FluentValidation.Results.ValidationFailure error)
        {
            return $"custom.{error.PropertyName}";
        }
    }
}
