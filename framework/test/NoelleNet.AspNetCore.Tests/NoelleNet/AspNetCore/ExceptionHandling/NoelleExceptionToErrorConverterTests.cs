using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using NoelleNet.AspNetCore.ExceptionHandling.Localization;
using NoelleNet.ExceptionHandling;
using NoelleNet.Http;
using NoelleNet.Validation;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace NoelleNet.AspNetCore.ExceptionHandling;

public class NoelleExceptionToErrorConverterTests
{
    private readonly Mock<IStringLocalizer<NoelleExceptionHandlingResource>> _localizerMock;
    private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
    private readonly Mock<IOptions<NoelleExceptionLocalizationOptions>> _localizationOptionsMock;
    private readonly NoelleExceptionToErrorConverter _converter;

    public NoelleExceptionToErrorConverterTests()
    {
        _localizerMock = new Mock<IStringLocalizer<NoelleExceptionHandlingResource>>();
        _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
        _localizationOptionsMock = new Mock<IOptions<NoelleExceptionLocalizationOptions>>();
        _localizationOptionsMock.Setup(o => o.Value).Returns(new NoelleExceptionLocalizationOptions());

        SetupLocalizerDefaults();

        _converter = new NoelleExceptionToErrorConverter(
            _localizerMock.Object,
            _localizerFactoryMock.Object,
            _localizationOptionsMock.Object);
    }

    private void SetupLocalizerDefaults()
    {
        _localizerMock.Setup(l => l["InternalServerErrorMessage"]).Returns(new LocalizedString("InternalServerErrorMessage", "Internal server error"));
        _localizerMock.Setup(l => l["ValidationFailedErrorMessage"]).Returns(new LocalizedString("ValidationFailedErrorMessage", "Validation failed"));
        _localizerMock.Setup(l => l["DBConcurrencyErrorMessage"]).Returns(new LocalizedString("DBConcurrencyErrorMessage", "Database concurrency error"));
        _localizerMock.Setup(l => l["EntityNotFoundErrorMessageWithoutId", It.IsAny<object[]>()]).Returns(new LocalizedString("EntityNotFoundErrorMessageWithoutId", "Entity not found without ID"));
        _localizerMock.Setup(l => l["EntityNotFoundErrorMessage", It.IsAny<object[]>()]).Returns(new LocalizedString("EntityNotFoundErrorMessage", "Entity '{0}' with id '{1}' not found"));
        _localizerMock.Setup(l => l["ValidationErrorDetailsTitle"]).Returns(new LocalizedString("ValidationErrorDetailsTitle", "Validation errors:"));
    }

    [Fact]
    public void Constructor_NullLocalizer_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleExceptionToErrorConverter(null!, _localizerFactoryMock.Object, _localizationOptionsMock.Object));
    }

    [Fact]
    public void Constructor_NullLocalizerFactory_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleExceptionToErrorConverter(_localizerMock.Object, null!, _localizationOptionsMock.Object));
    }

    [Fact]
    public void Constructor_NullLocalizationOptions_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleExceptionToErrorConverter(_localizerMock.Object, _localizerFactoryMock.Object, null!));
    }

    [Fact]
    public void Convert_GenericException_ShouldReturnInternalServerError()
    {
        var exception = new Exception("test error");

        var result = _converter.Convert(exception);

        Assert.Equal("Internal server error", result.Message);
    }

    [Fact]
    public void Convert_ExceptionWithErrorCode_ShouldSetCode()
    {
        var exception = new BusinessException("ERR-001", "business error");

        var result = _converter.Convert(exception);

        Assert.Equal("ERR-001", result.Code);
    }

    [Fact]
    public void Convert_NoelleRemoteCallExceptionWithError_ShouldReturnErrorInfo()
    {
        var error = new RemoteCallErrorInfo("remote error", code: "RC001", details: "remote details");
        var exception = new NoelleRemoteCallException(error);

        var result = _converter.Convert(exception);

        Assert.Equal("remote error", result.Message);
        Assert.Equal("RC001", result.Code);
        Assert.Equal("remote details", result.Details);
    }

    [Fact]
    public void Convert_DBConcurrencyException_ShouldReturnDBConcurrencyMessage()
    {
        var exception = new DBConcurrencyException();

        var result = _converter.Convert(exception);

        Assert.Equal("Database concurrency error", result.Message);
    }

    [Fact]
    public void Convert_EntityNotFoundExceptionWithTypeAndId_ShouldReturnLocalizedMessage()
    {
        var exception = new EntityNotFoundException(typeof(TestEntity), "123");

        var result = _converter.Convert(exception);

        Assert.Contains("not found", result.Message!);
    }

    [Fact]
    public void Convert_EntityNotFoundExceptionWithTypeOnly_ShouldReturnLocalizedMessageWithoutId()
    {
        var exception = new EntityNotFoundException(typeof(TestEntity));

        var result = _converter.Convert(exception);

        Assert.Equal("Entity not found without ID", result.Message);
    }

    [Fact]
    public void Convert_EntityNotFoundExceptionWithoutType_ShouldUseExceptionMessage()
    {
        var exception = new EntityNotFoundException("custom entity not found message");

        var result = _converter.Convert(exception);

        Assert.Equal("custom entity not found message", result.Message);
    }

    [Fact]
    public void Convert_NoelleValidationException_ShouldSetValidationErrors()
    {
        var validationResults = new List<ValidationResult>
        {
            new("Name is required", ["Name"]),
            new("Age must be positive", ["Age"])
        };
        var exception = new NoelleValidationException(validationResults);

        var result = _converter.Convert(exception);

        Assert.Equal("Validation failed", result.Message);
        Assert.NotNull(result.ValidationErrors);
        Assert.Equal(2, result.ValidationErrors.Count());
    }

    [Fact]
    public void Convert_NoelleRemoteCallException_ShouldPreserveMessage()
    {
        var exception = new NoelleRemoteCallException("remote call failed");

        var result = _converter.Convert(exception);

        Assert.Equal("remote call failed", result.Message);
    }

    [Fact]
    public void Convert_OptionsBuilderIncludeExceptionDetails_ShouldPopulateDetails()
    {
        var exception = new Exception("test error");

        var result = _converter.Convert(exception, options =>
        {
            options.IncludeExceptionDetails = true;
        });

        Assert.NotNull(result.Details);
        Assert.Contains("Exception", result.Details);
        Assert.Contains("test error", result.Details);
    }

    [Fact]
    public void Convert_OptionsBuilderIncludeStackTrace_ShouldIncludeStackTrace()
    {
        var exception = new Exception("test error");
        try { throw exception; } catch (Exception e) { exception = e; }

        var result = _converter.Convert(exception, options =>
        {
            options.IncludeExceptionDetails = true;
            options.IncludeStackTrace = true;
        });

        Assert.NotNull(result.Details);
        Assert.Contains("Stack Trace", result.Details);
    }

    [Fact]
    public void Convert_OptionsBuilderIncludeExceptionData_ShouldCopyData()
    {
        var exception = new Exception("test error");
        exception.Data["key1"] = "value1";

        var result = _converter.Convert(exception, options =>
        {
            options.IncludeExceptionData = true;
        });

        Assert.NotNull(result.Data);
        Assert.Equal("value1", result.Data["key1"]);
    }

    [Fact]
    public void Convert_AggregateException_ShouldIncludeInnerExceptionsInDetails()
    {
        var inner1 = new Exception("inner error 1");
        var inner2 = new Exception("inner error 2");
        var exception = new AggregateException(inner1, inner2);

        var result = _converter.Convert(exception, options =>
        {
            options.IncludeExceptionDetails = true;
        });

        Assert.NotNull(result.Details);
        Assert.Contains("inner error 1", result.Details);
        Assert.Contains("inner error 2", result.Details);
    }

    [Fact]
    public void Convert_ExceptionWithInnerException_ShouldIncludeInnerInDetails()
    {
        var innerException = new Exception("inner error");
        var exception = new Exception("outer error", innerException);

        var result = _converter.Convert(exception, options =>
        {
            options.IncludeExceptionDetails = true;
        });

        Assert.NotNull(result.Details);
        Assert.Contains("inner error", result.Details);
    }

    [Fact]
    public void Convert_ValidationExceptionIncludeDetailsFalse_ShouldHaveValidationDetails()
    {
        var validationResults = new List<ValidationResult>
        {
            new("Name is required", ["Name"])
        };
        var exception = new NoelleValidationException(validationResults);

        var result = _converter.Convert(exception);

        Assert.NotNull(result.Details);
        Assert.Contains("Validation errors", result.Details);
    }

    [Fact]
    public void Convert_LocalizedMessage_ShouldUseLocalizerProvider()
    {
        var exception = new BusinessException("ERR_CUSTOM", "fallback message");
        var customLocalizerMock = new Mock<IStringLocalizer>();
        customLocalizerMock.Setup(l => l["ERR_CUSTOM"]).Returns(new LocalizedString("ERR_CUSTOM", "Custom localized message"));

        _localizationOptionsMock.Setup(o => o.Value).Returns(new NoelleExceptionLocalizationOptions
        {
            LocalizerProvider = (ex, factory) => customLocalizerMock.Object
        });

        var converter = new NoelleExceptionToErrorConverter(
            _localizerMock.Object,
            _localizerFactoryMock.Object,
            _localizationOptionsMock.Object);

        var result = converter.Convert(exception);

        Assert.Equal("Custom localized message", result.Message);
        Assert.Equal("ERR_CUSTOM", result.Code);
    }

    [Fact]
    public void Convert_LocalizedMessageWithDataPlaceholders_ShouldReplace()
    {
        var exception = new BusinessException("ERR_TEMPLATE");
        exception.Data["Name"] = "TestName";

        var customLocalizerMock = new Mock<IStringLocalizer>();
        customLocalizerMock.Setup(l => l["ERR_TEMPLATE"]).Returns(new LocalizedString("ERR_TEMPLATE", "Error with {Name}"));

        _localizationOptionsMock.Setup(o => o.Value).Returns(new NoelleExceptionLocalizationOptions
        {
            LocalizerProvider = (ex, factory) => customLocalizerMock.Object
        });

        var converter = new NoelleExceptionToErrorConverter(
            _localizerMock.Object,
            _localizerFactoryMock.Object,
            _localizationOptionsMock.Object);

        var result = converter.Convert(exception);

        Assert.Equal("Error with TestName", result.Message);
    }

    [Fact]
    public void Convert_LocalizedMessageResourceNotFound_ShouldFallbackToDefault()
    {
        var exception = new BusinessException("ERR_UNKNOWN", "fallback");

        var customLocalizerMock = new Mock<IStringLocalizer>();
        customLocalizerMock.Setup(l => l["ERR_UNKNOWN"]).Returns(new LocalizedString("ERR_UNKNOWN", "", resourceNotFound: true));

        _localizationOptionsMock.Setup(o => o.Value).Returns(new NoelleExceptionLocalizationOptions
        {
            LocalizerProvider = (ex, factory) => customLocalizerMock.Object
        });

        var converter = new NoelleExceptionToErrorConverter(
            _localizerMock.Object,
            _localizerFactoryMock.Object,
            _localizationOptionsMock.Object);

        var result = converter.Convert(exception);

        Assert.Equal("Internal server error", result.Message);
    }

    [Fact]
    public void Convert_LocalizedMessageEmptyErrorCode_ShouldReturnDefault()
    {
        var exception = new BusinessException("", "fallback");

        var result = _converter.Convert(exception);

        // Empty error code - no localization attempted, falls to generic
        Assert.Equal("Internal server error", result.Message);
    }

    [Fact]
    public void Convert_LocalizedMessageNoLocalizerProvider_ShouldFallback()
    {
        var exception = new BusinessException("ERR_NOLOC", "fallback");

        _localizationOptionsMock.Setup(o => o.Value).Returns(new NoelleExceptionLocalizationOptions
        {
            LocalizerProvider = null
        });

        var converter = new NoelleExceptionToErrorConverter(
            _localizerMock.Object,
            _localizerFactoryMock.Object,
            _localizationOptionsMock.Object);

        var result = converter.Convert(exception);

        Assert.Equal("Internal server error", result.Message);
        Assert.Equal("ERR_NOLOC", result.Code);
    }

    [Fact]
    public void Convert_IHasErrorCodeEmptyCode_ShouldNotSetCode()
    {
        var exception = new BusinessException(null, "msg");

        var result = _converter.Convert(exception);

        Assert.Null(result.Code);
    }

    private class TestEntity
    {
    }
}
