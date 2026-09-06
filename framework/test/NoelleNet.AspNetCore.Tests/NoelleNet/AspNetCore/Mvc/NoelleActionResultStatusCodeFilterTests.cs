using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace NoelleNet.AspNetCore.Mvc;

/// <summary>
/// <see cref="NoelleActionResultStatusCodeFilter"/> 的契约测试：按 HTTP 方法补全状态码
/// </summary>
public class NoelleActionResultStatusCodeFilterTests
{
    private static (ResultExecutingContext Context, ActionContext ActionContext) CreateContext(string method, IActionResult result)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;
        httpContext.Request.Path = "/test";

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ResultExecutingContext(actionContext, [], result, controller: null!);

        return (context, actionContext);
    }

    private static ResultExecutionDelegate CreateNext(ActionContext actionContext)
    {
        return () => Task.FromResult(new ResultExecutedContext(actionContext, [], new EmptyResult(), controller: null!));
    }

    /// <summary>
    /// EmptyResult 在 GET/POST/PUT/PATCH/DELETE 下应转为 204 NoContent
    /// </summary>
    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
    public async Task OnResultExecutionAsync_EmptyResult_ShouldSetNoContent(string method)
    {
        var (context, actionContext) = CreateContext(method, new EmptyResult());

        await new NoelleActionResultStatusCodeFilter().OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.IsType<NoContentResult>(context.Result);
    }

    /// <summary>
    /// POST 无状态码的 ObjectResult 应转为 201；PUT/PATCH/DELETE 应转为 200
    /// </summary>
    [Fact]
    public async Task OnResultExecutionAsync_ObjectResultWithoutStatus_ShouldSetDefaultStatus()
    {
        var (postContext, postAction) = CreateContext("POST", new ObjectResult("created"));
        await new NoelleActionResultStatusCodeFilter().OnResultExecutionAsync(postContext, CreateNext(postAction));
        Assert.Equal(StatusCodes.Status201Created, Assert.IsType<JsonResult>(postContext.Result).StatusCode);

        foreach (var method in new[] { "PUT", "PATCH", "DELETE" })
        {
            var (context, action) = CreateContext(method, new ObjectResult("data"));
            await new NoelleActionResultStatusCodeFilter().OnResultExecutionAsync(context, CreateNext(action));
            Assert.IsType<OkObjectResult>(context.Result);
        }
    }

    /// <summary>
    /// 已有状态码的 ObjectResult 与 GET 的 ObjectResult 应保持不变
    /// </summary>
    [Fact]
    public async Task OnResultExecutionAsync_ResultWithStatusOrGet_ShouldNotChange()
    {
        var result = new ObjectResult("created") { StatusCode = 200 };
        var (context, action) = CreateContext("POST", result);
        await new NoelleActionResultStatusCodeFilter().OnResultExecutionAsync(context, CreateNext(action));
        Assert.Same(result, context.Result);

        var getResult = new ObjectResult("data");
        var (getContext, getAction) = CreateContext("GET", getResult);
        await new NoelleActionResultStatusCodeFilter().OnResultExecutionAsync(getContext, CreateNext(getAction));
        Assert.Same(getResult, getContext.Result);
    }

    /// <summary>
    /// CONNECT/HEAD/OPTIONS/TRACE 等非常规方法应不做处理
    /// </summary>
    [Theory]
    [InlineData("CONNECT")]
    [InlineData("HEAD")]
    [InlineData("OPTIONS")]
    [InlineData("TRACE")]
    public async Task OnResultExecutionAsync_UnhandledMethods_ShouldNotChange(string method)
    {
        var (context, action) = CreateContext(method, new EmptyResult());

        await new NoelleActionResultStatusCodeFilter().OnResultExecutionAsync(context, CreateNext(action));

        Assert.IsType<EmptyResult>(context.Result);
    }

    /// <summary>
    /// 过滤器应继续执行后续管道
    /// </summary>
    [Fact]
    public async Task OnResultExecutionAsync_ShouldInvokeNext()
    {
        var (context, actionContext) = CreateContext("GET", new EmptyResult());
        bool nextInvoked = false;

        await new NoelleActionResultStatusCodeFilter().OnResultExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ResultExecutedContext(actionContext, [], new EmptyResult(), controller: null!));
        });

        Assert.True(nextInvoked);
    }
}
