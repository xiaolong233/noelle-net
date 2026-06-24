using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using NoelleNet.AspNetCore.Validation.Localization;
using NoelleNet.Validation;

namespace NoelleNet.AspNetCore.Validation;

public class NoelleFluentValidationFilterTests
{
    [Fact]
    public async Task OnActionExecutionAsync_NoValidators_ShouldInvokeNext()
    {
        var localizerMock = new Mock<IStringLocalizer<NoelleValidationResource>>();
        var services = new ServiceCollection()
            .AddSingleton(localizerMock.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        var parameters = new List<ParameterDescriptor>
        {
            new ParameterDescriptor { Name = "input", ParameterType = typeof(TestModel) }
        };
        var actionDescriptor = new ActionDescriptor { Parameters = parameters };
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?> { ["input"] = new TestModel() },
            controller: null!);

        var filter = new NoelleFluentValidationFilter();
        bool nextInvoked = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, [], controller: null!));
        });

        Assert.True(nextInvoked);
    }

    [Fact]
    public async Task OnActionExecutionAsync_ValidationFails_ShouldThrowNoelleValidationException()
    {
        var localizerMock = new Mock<IStringLocalizer<NoelleValidationResource>>();
        var services = new ServiceCollection()
            .AddSingleton<IValidator<TestModel>, TestModelValidator>()
            .AddSingleton(localizerMock.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        var parameters = new List<ParameterDescriptor>
        {
            new ParameterDescriptor { Name = "input", ParameterType = typeof(TestModel) }
        };
        var actionDescriptor = new ActionDescriptor { Parameters = parameters };
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?> { ["input"] = new TestModel { Name = "" } },
            controller: null!);

        var filter = new NoelleFluentValidationFilter();

        await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(actionContext, [], controller: null!))));
    }

    [Fact]
    public async Task OnActionExecutionAsync_ValidationPasses_ShouldInvokeNext()
    {
        var localizerMock = new Mock<IStringLocalizer<NoelleValidationResource>>();
        var services = new ServiceCollection()
            .AddSingleton<IValidator<TestModel>, TestModelValidator>()
            .AddSingleton(localizerMock.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        var parameters = new List<ParameterDescriptor>
        {
            new ParameterDescriptor { Name = "input", ParameterType = typeof(TestModel) }
        };
        var actionDescriptor = new ActionDescriptor { Parameters = parameters };
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?> { ["input"] = new TestModel { Name = "valid" } },
            controller: null!);

        var filter = new NoelleFluentValidationFilter();
        bool nextInvoked = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, [], controller: null!));
        });

        Assert.True(nextInvoked);
    }

    [Fact]
    public async Task OnActionExecutionAsync_NullParamWithNullableType_ShouldNotAddPreValidationError()
    {
        var localizerMock = new Mock<IStringLocalizer<NoelleValidationResource>>();
        localizerMock.Setup(l => l["ParameterRequiredErrorMessage"]).Returns(new LocalizedString("ParameterRequiredErrorMessage", "Parameter is required"));
        localizerMock.Setup(l => l["RequestBodyRequiredErrorMessage"]).Returns(new LocalizedString("RequestBodyRequiredErrorMessage", "Request body is required"));

        var services = new ServiceCollection()
            .AddSingleton(localizerMock.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        var parameters = new List<ParameterDescriptor>
        {
            new ParameterDescriptor { Name = "input", ParameterType = typeof(int?) }
        };
        var actionDescriptor = new ActionDescriptor { Parameters = parameters };
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            controller: null!);

        var filter = new NoelleFluentValidationFilter();
        bool nextInvoked = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, [], controller: null!));
        });

        Assert.True(nextInvoked);
    }

    [Fact]
    public async Task OnActionExecutionAsync_NullParamWithNonNullableType_ShouldThrowValidationException()
    {
        var localizerMock = new Mock<IStringLocalizer<NoelleValidationResource>>();
        localizerMock.Setup(l => l["ParameterRequiredErrorMessage"]).Returns(new LocalizedString("ParameterRequiredErrorMessage", "Parameter '{0}' is required"));
        localizerMock.Setup(l => l["RequestBodyRequiredErrorMessage"]).Returns(new LocalizedString("RequestBodyRequiredErrorMessage", "Request body is required"));

        var services = new ServiceCollection()
            .AddSingleton(localizerMock.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        var parameters = new List<ParameterDescriptor>
        {
            new ParameterDescriptor { Name = "input", ParameterType = typeof(TestModel) }
        };
        var actionDescriptor = new ActionDescriptor { Parameters = parameters };
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            controller: null!);

        var filter = new NoelleFluentValidationFilter();

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(actionContext, [], controller: null!))));

        Assert.Equal(2, exception.ValidationResults.Count());
        Assert.Contains(exception.ValidationResults, r => r.ErrorMessage!.Contains("Request body"));
        Assert.Contains(exception.ValidationResults, r => r.ErrorMessage!.Contains("input"));
    }

    [Fact]
    public async Task OnActionExecutionAsync_ValidationErrorShouldContainMemberNames()
    {
        var localizerMock = new Mock<IStringLocalizer<NoelleValidationResource>>();
        var services = new ServiceCollection()
            .AddSingleton<IValidator<TestModel>, TestModelValidator>()
            .AddSingleton(localizerMock.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        var parameters = new List<ParameterDescriptor>
        {
            new ParameterDescriptor { Name = "input", ParameterType = typeof(TestModel) }
        };
        var actionDescriptor = new ActionDescriptor { Parameters = parameters };
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?> { ["input"] = new TestModel { Name = "" } },
            controller: null!);

        var filter = new NoelleFluentValidationFilter();

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => filter.OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(actionContext, [], controller: null!))));

        var firstError = exception.ValidationResults.First();
        Assert.Contains("Name", firstError.MemberNames);
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
}
