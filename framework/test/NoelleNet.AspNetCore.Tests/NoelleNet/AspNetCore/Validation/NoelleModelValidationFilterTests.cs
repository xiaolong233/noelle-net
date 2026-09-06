using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using NoelleNet.Validation;

namespace NoelleNet.AspNetCore.Validation;

/// <summary>
/// <see cref="NoelleModelValidationFilter"/> 的行为测试：ModelState 无效时转为统一验证异常
/// </summary>
public class NoelleModelValidationFilterTests
{
    private static ActionExecutingContext CreateContext(ModelStateDictionary modelState)
    {
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            controller: null!);
        context.ModelState.Merge(modelState);
        return context;
    }

    /// <summary>
    /// ModelState 有效时应继续执行后续管道
    /// </summary>
    [Fact]
    public async Task OnActionExecutionAsync_ModelStateValid_ShouldInvokeNext()
    {
        var context = CreateContext(new ModelStateDictionary());
        bool nextInvoked = false;

        await new NoelleModelValidationFilter().OnActionExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ActionExecutedContext(context, [], controller: null!));
        });

        Assert.True(nextInvoked);
    }

    /// <summary>
    /// ModelState 无效时应抛 NoelleValidationException，收集全部错误并携带成员名
    /// </summary>
    [Fact]
    public async Task OnActionExecutionAsync_ModelStateInvalid_ShouldThrowValidationExceptionWithAllErrors()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Name", "Name is required");
        modelState.AddModelError("Age", "Age must be positive");
        var context = CreateContext(modelState);

        var exception = await Assert.ThrowsAsync<NoelleValidationException>(
            () => new NoelleModelValidationFilter().OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(context, [], controller: null!))));

        Assert.Equal(2, exception.ValidationResults.Count());
        Assert.Contains(exception.ValidationResults, r => r.ErrorMessage == "Name is required");
        Assert.Contains(exception.ValidationResults, r => r.MemberNames.Contains("Name"));
    }
}
