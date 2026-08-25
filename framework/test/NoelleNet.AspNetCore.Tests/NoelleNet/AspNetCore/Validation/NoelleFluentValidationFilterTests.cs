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

public class NoelleFluentValidationFilterTests
{
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

    public class TestModelAnotherValidator : AbstractValidator<TestModel>
    {
        public TestModelAnotherValidator()
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
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor { Parameters = [parameter] });
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

    [Fact]
    public async Task OnActionExecutionAsync_ValidModel_ShouldInvokeNext()
    {
        var filter = CreateFilter();
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestModel>, TestModelValidator>();
        var context = CreateContext(new TestModel { Name = "Noelle" }, CreateParameter(typeof(TestModel)), services.BuildServiceProvider());
        bool nextInvoked = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ActionExecutedContext(context, [], controller: null!));
        });

        Assert.True(nextInvoked);
    }

    [Fact]
    public async Task OnActionExecutionAsync_InvalidModel_ShouldThrowNoelleValidationException()
    {
        var filter = CreateFilter();
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestModel>, TestModelValidator>();
        var context = CreateContext(new TestModel { Name = null }, CreateParameter(typeof(TestModel)), services.BuildServiceProvider());

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(context, [], controller: null!))));

        var result = Assert.Single(exception.ValidationResults);
        Assert.Equal("Name is required", result.ErrorMessage);
        Assert.Contains("Name", result.MemberNames);
    }

    [Fact]
    public async Task OnActionExecutionAsync_NullArgument_ShouldThrowParameterRequiredError()
    {
        var filter = CreateFilter();
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestModel>, TestModelValidator>();
        var context = CreateContext(null, CreateParameter(typeof(TestModel)), services.BuildServiceProvider());

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(context, [], controller: null!))));

        var result = Assert.Single(exception.ValidationResults);
        Assert.Equal("The parameter is required.", result.ErrorMessage);
        Assert.Contains("model", result.MemberNames);
    }

    [Fact]
    public async Task OnActionExecutionAsync_NullArgumentWithEmptyBodyBehaviorAllow_ShouldInvokeNext()
    {
        var filter = CreateFilter();
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestModel>, TestModelValidator>();
        var parameter = CreateParameter(typeof(TestModel));
        parameter.BindingInfo = new BindingInfo { EmptyBodyBehavior = EmptyBodyBehavior.Allow };
        var context = CreateContext(null, parameter, services.BuildServiceProvider());
        bool nextInvoked = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ActionExecutedContext(context, [], controller: null!));
        });

        Assert.True(nextInvoked);
    }

    [Fact]
    public async Task OnActionExecutionAsync_UnboundArgument_ShouldThrowParameterRequiredError()
    {
        var filter = CreateFilter();
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestModel>, TestModelValidator>();
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor { Parameters = [CreateParameter(typeof(TestModel))] });
        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            controller: null!);

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(context, [], controller: null!))));

        var result = Assert.Single(exception.ValidationResults);
        Assert.Equal("The parameter is required.", result.ErrorMessage);
        Assert.Contains("model", result.MemberNames);
    }

    [Fact]
    public async Task OnActionExecutionAsync_ParameterWithoutValidator_ShouldInvokeNext()
    {
        var filter = CreateFilter();
        var services = new ServiceCollection();
        var context = CreateContext("Noelle", CreateParameter(typeof(string)), services.BuildServiceProvider());
        bool nextInvoked = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ActionExecutedContext(context, [], controller: null!));
        });

        Assert.True(nextInvoked);
    }

    [Fact]
    public async Task OnActionExecutionAsync_MultipleValidators_ShouldValidateWithAllValidators()
    {
        var filter = CreateFilter();
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestModel>, TestModelValidator>();
        services.AddScoped<IValidator<TestModel>, TestModelAnotherValidator>();
        var context = CreateContext(new TestModel { Name = "A" }, CreateParameter(typeof(TestModel)), services.BuildServiceProvider());

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(context, [], controller: null!))));

        var result = Assert.Single(exception.ValidationResults);
        Assert.Equal("Name must be at least 3 characters", result.ErrorMessage);
        Assert.Contains("Name", result.MemberNames);
    }

    [Fact]
    public async Task OnActionExecutionAsync_MultipleValidators_ShouldCollectAllErrors()
    {
        var filter = CreateFilter();
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestModel>, TestModelValidator>();
        services.AddScoped<IValidator<TestModel>, TestModelAnotherValidator>();
        var context = CreateContext(new TestModel { Name = string.Empty }, CreateParameter(typeof(TestModel)), services.BuildServiceProvider());

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(context, [], controller: null!))));

        Assert.Equal(2, exception.ValidationResults.Count());
    }

    [Fact]
    public async Task OnActionExecutionAsync_ModelLevelError_ShouldContainEmptyMemberName()
    {
        var filter = CreateFilter();
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestModel>, TestModelLevelValidator>();
        var context = CreateContext(new TestModel { Name = "Noelle" }, CreateParameter(typeof(TestModel)), services.BuildServiceProvider());

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(context, [], controller: null!))));

        var result = Assert.Single(exception.ValidationResults);
        Assert.Equal(string.Empty, Assert.Single(result.MemberNames));
    }

    [Fact]
    public async Task OnActionExecutionAsync_OverriddenGetMemberName_ShouldUseCustomMemberName()
    {
        var localizer = new Mock<IStringLocalizer<NoelleValidationResource>>();
        var filter = new CustomMemberNameFilter(localizer.Object);
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestModel>, TestModelValidator>();
        var context = CreateContext(new TestModel { Name = null }, CreateParameter(typeof(TestModel)), services.BuildServiceProvider());

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(context, [], controller: null!))));

        var result = Assert.Single(exception.ValidationResults);
        Assert.Equal("custom.Name", Assert.Single(result.MemberNames));
    }

    [Fact]
    public void Constructor_NullLocalizer_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new NoelleFluentValidationFilter(null!));
    }

    [Fact]
    public async Task OnActionExecutionAsync_CancellationTokenParameter_ShouldInvokeNext()
    {
        var filter = CreateFilter();
        var services = new ServiceCollection();
        var context = CreateContext(null, CreateParameter(typeof(CancellationToken)), services.BuildServiceProvider());
        bool nextInvoked = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ActionExecutedContext(context, [], controller: null!));
        });

        Assert.True(nextInvoked);
    }

    [Fact]
    public async Task OnActionExecutionAsync_FromServicesParameter_ShouldInvokeNext()
    {
        var filter = CreateFilter();
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestModel>, TestModelValidator>();
        var parameter = CreateParameter(typeof(TestModel));
        parameter.BindingInfo = new BindingInfo { BindingSource = BindingSource.Services };
        var context = CreateContext(null, parameter, services.BuildServiceProvider());
        bool nextInvoked = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ActionExecutedContext(context, [], controller: null!));
        });

        Assert.True(nextInvoked);
    }

    [Fact]
    public async Task OnActionExecutionAsync_BindingSourceSpecialParameter_ShouldInvokeNext()
    {
        var filter = CreateFilter();
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestModel>, TestModelValidator>();
        var parameter = CreateParameter(typeof(TestModel));
        parameter.BindingInfo = new BindingInfo { BindingSource = BindingSource.Special };
        var context = CreateContext(null, parameter, services.BuildServiceProvider());
        bool nextInvoked = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ActionExecutedContext(context, [], controller: null!));
        });

        Assert.True(nextInvoked);
    }
}
