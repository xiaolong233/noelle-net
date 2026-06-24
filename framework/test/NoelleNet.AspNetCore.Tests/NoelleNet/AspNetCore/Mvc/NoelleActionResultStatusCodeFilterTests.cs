using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace NoelleNet.AspNetCore.Mvc;

public class NoelleActionResultStatusCodeFilterTests
{
    private static (ResultExecutingContext Context, ActionContext ActionContext) CreateContext(string method, IActionResult result)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;
        httpContext.Request.Path = "/test";

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        var context = new ResultExecutingContext(
            actionContext,
            [new NoelleActionResultStatusCodeFilter()],
            result,
            controller: null!);

        return (context, actionContext);
    }

    private static ResultExecutionDelegate CreateNext(ActionContext actionContext)
    {
        return () => Task.FromResult(new ResultExecutedContext(actionContext, [], new EmptyResult(), controller: null!));
    }

    [Fact]
    public async Task OnResultExecutionAsync_GetEmptyResult_ShouldSetNoContent()
    {
        var (context, actionContext) = CreateContext(HttpMethods.Get, new EmptyResult());

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.IsType<NoContentResult>(context.Result);
    }

    [Fact]
    public async Task OnResultExecutionAsync_GetObjectResult_ShouldNotChange()
    {
        var result = new ObjectResult("data");
        var (context, actionContext) = CreateContext(HttpMethods.Get, result);

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.Same(result, context.Result);
        Assert.Null(result.StatusCode);
    }

    [Fact]
    public async Task OnResultExecutionAsync_PostEmptyResult_ShouldSetNoContent()
    {
        var (context, actionContext) = CreateContext(HttpMethods.Post, new EmptyResult());

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.IsType<NoContentResult>(context.Result);
    }

    [Fact]
    public async Task OnResultExecutionAsync_PostObjectResultNoStatus_ShouldSet201()
    {
        var result = new ObjectResult("created");
        var (context, actionContext) = CreateContext(HttpMethods.Post, result);

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        var jsonResult = Assert.IsType<JsonResult>(context.Result);
        Assert.Equal(StatusCodes.Status201Created, jsonResult.StatusCode);
    }

    [Fact]
    public async Task OnResultExecutionAsync_PostObjectResultWithStatus_ShouldNotChange()
    {
        var result = new ObjectResult("created") { StatusCode = 200 };
        var (context, actionContext) = CreateContext(HttpMethods.Post, result);

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.Same(result, context.Result);
    }

    [Fact]
    public async Task OnResultExecutionAsync_PutEmptyResult_ShouldSetNoContent()
    {
        var (context, actionContext) = CreateContext(HttpMethods.Put, new EmptyResult());

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.IsType<NoContentResult>(context.Result);
    }

    [Fact]
    public async Task OnResultExecutionAsync_PutObjectResultNoStatus_ShouldSetOk()
    {
        var result = new ObjectResult("updated");
        var (context, actionContext) = CreateContext(HttpMethods.Put, result);

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.IsType<OkObjectResult>(context.Result);
    }

    [Fact]
    public async Task OnResultExecutionAsync_PatchEmptyResult_ShouldSetNoContent()
    {
        var (context, actionContext) = CreateContext(HttpMethods.Patch, new EmptyResult());

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.IsType<NoContentResult>(context.Result);
    }

    [Fact]
    public async Task OnResultExecutionAsync_PatchObjectResultNoStatus_ShouldSetOk()
    {
        var result = new ObjectResult("patched");
        var (context, actionContext) = CreateContext(HttpMethods.Patch, result);

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.IsType<OkObjectResult>(context.Result);
    }

    [Fact]
    public async Task OnResultExecutionAsync_DeleteEmptyResult_ShouldSetNoContent()
    {
        var (context, actionContext) = CreateContext(HttpMethods.Delete, new EmptyResult());

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.IsType<NoContentResult>(context.Result);
    }

    [Fact]
    public async Task OnResultExecutionAsync_DeleteObjectResultNoStatus_ShouldSetOk()
    {
        var result = new ObjectResult("deleted");
        var (context, actionContext) = CreateContext(HttpMethods.Delete, result);

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.IsType<OkObjectResult>(context.Result);
    }

    [Fact]
    public async Task OnResultExecutionAsync_Connect_ShouldNotChange()
    {
        var result = new EmptyResult();
        var (context, actionContext) = CreateContext(HttpMethods.Connect, result);

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.IsType<EmptyResult>(context.Result);
    }

    [Fact]
    public async Task OnResultExecutionAsync_Head_ShouldNotChange()
    {
        var result = new EmptyResult();
        var (context, actionContext) = CreateContext(HttpMethods.Head, result);

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.IsType<EmptyResult>(context.Result);
    }

    [Fact]
    public async Task OnResultExecutionAsync_Options_ShouldNotChange()
    {
        var result = new EmptyResult();
        var (context, actionContext) = CreateContext(HttpMethods.Options, result);

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.IsType<EmptyResult>(context.Result);
    }

    [Fact]
    public async Task OnResultExecutionAsync_Trace_ShouldNotChange()
    {
        var result = new EmptyResult();
        var (context, actionContext) = CreateContext(HttpMethods.Trace, result);

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, CreateNext(actionContext));

        Assert.IsType<EmptyResult>(context.Result);
    }

    [Fact]
    public async Task OnResultExecutionAsync_ShouldInvokeNext()
    {
        var result = new EmptyResult();
        var (context, actionContext) = CreateContext(HttpMethods.Get, result);
        bool nextInvoked = false;

        var filter = new NoelleActionResultStatusCodeFilter();
        await filter.OnResultExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ResultExecutedContext(actionContext, [], new EmptyResult(), controller: null!));
        });

        Assert.True(nextInvoked);
    }
}
