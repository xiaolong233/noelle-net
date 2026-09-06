using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using NoelleNet.AspNetCore.ExceptionHandling.Localization;
using NoelleNet.ExceptionHandling;
using NoelleNet.Validation;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace NoelleNet.AspNetCore.ExceptionHandling;

public class ProblemDetailsErrorResponseWriterTests
{
    private readonly Mock<IProblemDetailsService> _problemDetailsServiceMock;
    private ProblemDetailsContext? _capturedContext;

    public ProblemDetailsErrorResponseWriterTests()
    {
        _problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        _problemDetailsServiceMock
            .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(ctx => _capturedContext = ctx)
            .ReturnsAsync(true);
    }

    private ProblemDetailsErrorResponseWriter CreateWriter(
        NoelleExceptionHandlingOptions? options = null,
        NoelleExceptionLocalizationOptions? localizationOptions = null,
        IStringLocalizer<NoelleExceptionHandlingResource>? localizer = null)
    {
        options ??= new NoelleExceptionHandlingOptions();
        localizationOptions ??= new NoelleExceptionLocalizationOptions();
        localizer ??= new FakeStringLocalizer();

        return new ProblemDetailsErrorResponseWriter(
            Options.Create(options),
            Options.Create(localizationOptions),
            Mock.Of<IStringLocalizerFactory>(),
            localizer);
    }

    private HttpContext CreateHttpContext()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_problemDetailsServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = services.BuildServiceProvider();
        return httpContext;
    }

    [Fact]
    public async Task TryWriteAsync_GenericException_ShouldWrite500ProblemDetails()
    {
        var httpContext = CreateHttpContext();
        var exception = new Exception("boom");
        var writer = CreateWriter();

        bool result = await writer.TryWriteAsync(httpContext, exception);

        Assert.True(result);
        Assert.NotNull(_capturedContext);
        Assert.Same(httpContext, _capturedContext!.HttpContext);
        Assert.Same(exception, _capturedContext.Exception);
        Assert.Equal(StatusCodes.Status500InternalServerError, _capturedContext.ProblemDetails.Status);
        Assert.Equal(NoelleProblemDetailsTypes.InternalServerError, _capturedContext.ProblemDetails.Type);
        Assert.Equal("InternalServerErrorMessage", _capturedContext.ProblemDetails.Title);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryWriteAsync_BusinessException_ShouldSetResponseStatusCode()
    {
        var httpContext = CreateHttpContext();
        var writer = CreateWriter();

        await writer.TryWriteAsync(httpContext, new BusinessException("ERR001", "business error"));

        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryWriteAsync_BusinessException_ShouldWrite400WithErrorCode()
    {
        var writer = CreateWriter();
        var exception = new BusinessException("ERR001", "business error");

        await writer.TryWriteAsync(CreateHttpContext(), exception);

        Assert.NotNull(_capturedContext);
        Assert.Equal(StatusCodes.Status400BadRequest, _capturedContext!.ProblemDetails.Status);
        Assert.Equal(NoelleProblemDetailsTypes.BadRequest, _capturedContext.ProblemDetails.Type);
        Assert.Equal("BadRequestErrorMessage", _capturedContext.ProblemDetails.Title);
        Assert.Equal("ERR001", _capturedContext.ProblemDetails.Extensions["code"]);
    }

    [Fact]
    public async Task TryWriteAsync_BusinessExceptionWithDetails_ShouldOverrideDetail()
    {
        var writer = CreateWriter();
        var exception = new BusinessException("ERR001", "business error", "补充说明");

        await writer.TryWriteAsync(CreateHttpContext(), exception);

        Assert.NotNull(_capturedContext);
        Assert.Equal("补充说明", _capturedContext!.ProblemDetails.Detail);
    }

    [Fact]
    public async Task TryWriteAsync_EntityNotFoundExceptionWithoutEntityType_ShouldWrite404()
    {
        var writer = CreateWriter();
        var exception = new EntityNotFoundException("not found");

        await writer.TryWriteAsync(CreateHttpContext(), exception);

        Assert.NotNull(_capturedContext);
        Assert.Equal(StatusCodes.Status404NotFound, _capturedContext!.ProblemDetails.Status);
        Assert.Equal(NoelleProblemDetailsTypes.NotFound, _capturedContext.ProblemDetails.Type);
        Assert.Equal("EntityNotFoundErrorMessage", _capturedContext.ProblemDetails.Title);
        Assert.Null(_capturedContext.ProblemDetails.Detail);
    }

    [Fact]
    public async Task TryWriteAsync_EntityNotFoundExceptionWithEntityType_ShouldWriteDetail()
    {
        var writer = CreateWriter();
        var exception = new EntityNotFoundException<TestEntity>(42);

        await writer.TryWriteAsync(CreateHttpContext(), exception);

        Assert.NotNull(_capturedContext);
        Assert.Equal("EntityNotFoundErrorMessage", _capturedContext!.ProblemDetails.Title);
        Assert.Equal("EntityNotFoundErrorDetail", _capturedContext.ProblemDetails.Detail);
    }

    [Fact]
    public async Task TryWriteAsync_ValidationException_ShouldWriteErrors()
    {
        var writer = CreateWriter();
        var exception = new NoelleValidationException(
        [
            new ValidationResult("名称不能为空", ["Name"]),
            new ValidationResult("整体校验失败")
        ]);

        await writer.TryWriteAsync(CreateHttpContext(), exception);

        Assert.NotNull(_capturedContext);
        Assert.Equal(StatusCodes.Status400BadRequest, _capturedContext!.ProblemDetails.Status);
        Assert.Equal("ValidationFailedErrorMessage", _capturedContext.ProblemDetails.Title);
        var errors = Assert.IsType<Dictionary<string, List<string>>>(_capturedContext.ProblemDetails.Extensions["errors"]);
        Assert.Equal(["名称不能为空"], errors["Name"]);
        Assert.Equal(["整体校验失败"], errors[string.Empty]);
    }

    [Fact]
    public async Task TryWriteAsync_DBConcurrencyException_ShouldWrite409()
    {
        var writer = CreateWriter();

        await writer.TryWriteAsync(CreateHttpContext(), new DBConcurrencyException());

        Assert.NotNull(_capturedContext);
        Assert.Equal(StatusCodes.Status409Conflict, _capturedContext!.ProblemDetails.Status);
        Assert.Equal(NoelleProblemDetailsTypes.Conflict, _capturedContext.ProblemDetails.Type);
        Assert.Equal("DBConcurrencyErrorMessage", _capturedContext.ProblemDetails.Title);
    }

    [Fact]
    public async Task TryWriteAsync_NotImplementedException_ShouldWrite501()
    {
        var writer = CreateWriter();

        await writer.TryWriteAsync(CreateHttpContext(), new NotImplementedException());

        Assert.NotNull(_capturedContext);
        Assert.Equal(StatusCodes.Status501NotImplemented, _capturedContext!.ProblemDetails.Status);
        Assert.Equal(NoelleProblemDetailsTypes.NotImplemented, _capturedContext.ProblemDetails.Type);
        Assert.Equal("NotImplementedErrorMessage", _capturedContext.ProblemDetails.Title);
    }

    [Fact]
    public async Task TryWriteAsync_CustomStatusCodeException_ShouldUseDeclaredStatusCode()
    {
        var writer = CreateWriter();

        await writer.TryWriteAsync(CreateHttpContext(), new CustomStatusCodeException());

        Assert.NotNull(_capturedContext);
        Assert.Equal(418, _capturedContext!.ProblemDetails.Status);
        Assert.Equal(NoelleProblemDetailsTypes.Default, _capturedContext.ProblemDetails.Type);
        Assert.Equal("BadRequestErrorMessage", _capturedContext.ProblemDetails.Title);
    }

    [Fact]
    public async Task TryWriteAsync_IncludeExceptionData_ShouldWriteDataExtension()
    {
        var writer = CreateWriter(new NoelleExceptionHandlingOptions { IncludeExceptionData = true });
        var exception = new Exception("boom");
        exception.Data["retry"] = 5;
        exception.Data["name"] = "abc";

        await writer.TryWriteAsync(CreateHttpContext(), exception);

        Assert.NotNull(_capturedContext);
        var data = Assert.IsType<Dictionary<string, object?>>(_capturedContext!.ProblemDetails.Extensions["data"]);
        Assert.Equal(5, data["retry"]);
        Assert.Equal("abc", data["name"]);
    }

    [Fact]
    public async Task TryWriteAsync_IncludeExceptionDataDisabled_ShouldNotWriteDataExtension()
    {
        var writer = CreateWriter(new NoelleExceptionHandlingOptions { IncludeExceptionData = false });
        var exception = new Exception("boom");
        exception.Data["retry"] = 5;

        await writer.TryWriteAsync(CreateHttpContext(), exception);

        Assert.NotNull(_capturedContext);
        Assert.False(_capturedContext!.ProblemDetails.Extensions.ContainsKey("data"));
    }

    [Fact]
    public async Task TryWriteAsync_IncludeExceptionDetails_ShouldWriteExceptionExtension()
    {
        var writer = CreateWriter(new NoelleExceptionHandlingOptions { IncludeExceptionDetails = true });
        Exception exception;
        try
        {
            throw new InvalidOperationException("boom");
        }
        catch (Exception e)
        {
            exception = e;
        }

        await writer.TryWriteAsync(CreateHttpContext(), exception);

        Assert.NotNull(_capturedContext);
        var details = Assert.IsType<Dictionary<string, object?>>(_capturedContext!.ProblemDetails.Extensions["exception"]);
        Assert.Equal(typeof(InvalidOperationException).FullName, details["type"]);
        Assert.Equal("boom", details["message"]);
        Assert.False(details.ContainsKey("stackTrace"));
    }

    [Fact]
    public async Task TryWriteAsync_IncludeStackTrace_ShouldWriteStackTrace()
    {
        var writer = CreateWriter(new NoelleExceptionHandlingOptions
        {
            IncludeExceptionDetails = true,
            IncludeStackTrace = true
        });
        Exception exception;
        try
        {
            throw new InvalidOperationException("boom");
        }
        catch (Exception e)
        {
            exception = e;
        }

        await writer.TryWriteAsync(CreateHttpContext(), exception);

        Assert.NotNull(_capturedContext);
        var details = Assert.IsType<Dictionary<string, object?>>(_capturedContext!.ProblemDetails.Extensions["exception"]);
        Assert.True(details.ContainsKey("stackTrace"));
        Assert.False(string.IsNullOrWhiteSpace(details["stackTrace"] as string));
    }

    [Fact]
    public async Task TryWriteAsync_LocalizerProvider_ShouldWriteLocalizedTitle()
    {
        var localizer = new FakeStringLocalizer(new Dictionary<string, string>
        {
            ["ERR001"] = "商品 {name} 不存在"
        });
        var localizationOptions = new NoelleExceptionLocalizationOptions
        {
            LocalizerProvider = (_, _) => localizer
        };
        var writer = CreateWriter(localizationOptions: localizationOptions, localizer: localizer);
        var exception = new BusinessException("ERR001", "原消息");
        exception.Data["name"] = "苹果";

        await writer.TryWriteAsync(CreateHttpContext(), exception);

        Assert.NotNull(_capturedContext);
        Assert.Equal("商品 苹果 不存在", _capturedContext!.ProblemDetails.Title);
    }

    [Fact]
    public async Task TryWriteAsync_LocalizerProviderResourceNotFound_ShouldUseDefaultTitle()
    {
        var localizer = new FakeStringLocalizer();
        var localizationOptions = new NoelleExceptionLocalizationOptions
        {
            LocalizerProvider = (_, _) => localizer
        };
        var writer = CreateWriter(localizationOptions: localizationOptions, localizer: localizer);

        await writer.TryWriteAsync(CreateHttpContext(), new BusinessException("ERR001", "business error"));

        Assert.NotNull(_capturedContext);
        Assert.Equal("BadRequestErrorMessage", _capturedContext!.ProblemDetails.Title);
    }

    [Fact]
    public async Task TryWriteAsync_ServiceReturnsFalse_ShouldReturnFalse()
    {
        _problemDetailsServiceMock
            .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .ReturnsAsync(false);
        var writer = CreateWriter();

        bool result = await writer.TryWriteAsync(CreateHttpContext(), new Exception("boom"));

        Assert.False(result);
    }

    private sealed class CustomStatusCodeException : Exception, IHasHttpStatusCode
    {
        public int StatusCode => 418;
    }

    private sealed class TestEntity
    {
    }

    private sealed class FakeStringLocalizer : IStringLocalizer<NoelleExceptionHandlingResource>
    {
        private readonly Dictionary<string, string> _values;

        public FakeStringLocalizer() => _values = [];

        public FakeStringLocalizer(Dictionary<string, string> values) => _values = values;

        public LocalizedString this[string name] =>
            _values.TryGetValue(name, out var value)
                ? new LocalizedString(name, value)
                : new LocalizedString(name, name, resourceNotFound: true);

        public LocalizedString this[string name, params object[] arguments] => this[name];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
