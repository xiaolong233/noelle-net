using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using NoelleNet.Logging;

namespace NoelleNet.Validation;

public class NoelleValidationExceptionTests
{
    [Fact]
    public void Constructor_WithMessageAndValidationResults_ShouldInitialize()
    {
        var results = new[] { new ValidationResult("字段不能为空") };
        var ex = new NoelleValidationException("验证失败", results);

        Assert.Equal("验证失败", ex.Message);
        Assert.Same(results, ex.ValidationResults);
        Assert.Single(ex.ValidationResults);
    }

    [Fact]
    public void Constructor_WithValidationResultsOnly_ShouldInitialize()
    {
        var results = new[] { new ValidationResult("错误1"), new ValidationResult("错误2") };
        var ex = new NoelleValidationException(results);

        Assert.Equal(2, ex.ValidationResults.Count());
    }

    [Fact]
    public void Constructor_NullValidationResults_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new NoelleValidationException("msg", null!));
        Assert.Throws<ArgumentNullException>(() => new NoelleValidationException(null!));
    }

    [Fact]
    public void ShouldImplementInterfaces()
    {
        var results = new[] { new ValidationResult("error") };
        var ex = new NoelleValidationException(results);

        Assert.IsAssignableFrom<IHasValidationResults>(ex);
        Assert.IsAssignableFrom<IHasLogLevel>(ex);
    }

    [Fact]
    public void DefaultLogLevel_ShouldBeWarning()
    {
        var results = new[] { new ValidationResult("error") };
        var ex = new NoelleValidationException(results);

        Assert.Equal(LogLevel.Warning, ex.LogLevel);
    }

    [Fact]
    public void LogLevel_ShouldBeSettable()
    {
        var results = new[] { new ValidationResult("error") };
        var ex = new NoelleValidationException(results)
        {
            LogLevel = LogLevel.Error
        };

        Assert.Equal(LogLevel.Error, ex.LogLevel);
    }

    [Fact]
    public void ValidationResults_ShouldBeSettable()
    {
        var results = new[] { new ValidationResult("error") };
        var ex = new NoelleValidationException(results)
        {
            ValidationResults = new[] { new ValidationResult("new error") }
        };

        Assert.Single(ex.ValidationResults);
        Assert.Equal("new error", ex.ValidationResults.First().ErrorMessage);
    }
}
