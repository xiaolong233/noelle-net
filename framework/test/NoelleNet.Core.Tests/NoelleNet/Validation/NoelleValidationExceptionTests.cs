using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace NoelleNet.Validation;

/// <summary>
/// <see cref="NoelleValidationException"/> 的单元测试
/// </summary>
public class NoelleValidationExceptionTests
{
    /// <summary>
    /// 两种构造方式都应正确初始化验证结果
    /// </summary>
    [Fact]
    public void Constructors_ShouldInitializeValidationResults()
    {
        var results = new[] { new ValidationResult("错误1"), new ValidationResult("错误2") };

        var ex1 = new NoelleValidationException("验证失败", results);
        Assert.Equal("验证失败", ex1.Message);
        Assert.Same(results, ex1.ValidationResults);

        var ex2 = new NoelleValidationException(results);
        Assert.Equal(2, ex2.ValidationResults.Count());
    }

    /// <summary>
    /// 验证结果为 null 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void Constructor_NullValidationResults_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new NoelleValidationException("msg", null!));
        Assert.Throws<ArgumentNullException>(() => new NoelleValidationException(null!));
    }

    /// <summary>
    /// 默认日志级别应为 Information：验证失败是客户端输入问题、服务端行为完全正确，
    /// 且发生量最大，不应占用"应当很少"的 Warning 通道
    /// </summary>
    [Fact]
    public void DefaultLogLevel_ShouldBeInformation()
    {
        var ex = new NoelleValidationException([new ValidationResult("error")]);

        Assert.Equal(LogLevel.Information, ex.LogLevel);
    }
}
