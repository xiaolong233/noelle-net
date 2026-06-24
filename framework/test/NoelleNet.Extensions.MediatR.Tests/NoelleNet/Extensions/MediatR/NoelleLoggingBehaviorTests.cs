using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace NoelleNet.Extensions.MediatR;

/// <summary>
/// <see cref="NoelleLoggingBehavior{TRequest, TResponse}"/> 的单元测试
/// </summary>
public class NoelleLoggingBehaviorTests
{
    #region Constructor

    /// <summary>
    /// 传入 null 的 ILogger 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new NoelleLoggingBehavior<TestRequest, TestResponse>(null!));
    }

    /// <summary>
    /// 传入有效的 ILogger 时应成功创建实例
    /// </summary>
    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateInstance()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleLoggingBehavior<TestRequest, TestResponse>>>();

        // Act
        var behavior = new NoelleLoggingBehavior<TestRequest, TestResponse>(mockLogger.Object);

        // Assert
        Assert.NotNull(behavior);
    }

    #endregion

    #region Interface Implementation

    /// <summary>
    /// NoelleLoggingBehavior 应实现 IPipelineBehavior&lt;,&gt; 接口
    /// </summary>
    [Fact]
    public void ShouldImplementIPipelineBehavior()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleLoggingBehavior<TestRequest, TestResponse>>>();

        // Act
        var behavior = new NoelleLoggingBehavior<TestRequest, TestResponse>(mockLogger.Object);

        // Assert
        Assert.IsAssignableFrom<IPipelineBehavior<TestRequest, TestResponse>>(behavior);
    }

    #endregion

    #region Handle

    /// <summary>
    /// Handle 应在处理前后各记录一次日志
    /// </summary>
    [Fact]
    public async Task Handle_ShouldLogBeforeAndAfter()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleLoggingBehavior<TestRequest, TestResponse>>>();
        var behavior = new NoelleLoggingBehavior<TestRequest, TestResponse>(mockLogger.Object);
        var request = new TestRequest { Data = "test-data" };
        var expectedResponse = new TestResponse { Result = "test-result" };
        var nextCalled = false;

        Task<TestResponse> Next(CancellationToken ct)
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        }

        // Act
        var response = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, response);
        Assert.True(nextCalled);

        // 验证记录了两次日志（开始和完成）
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => state.ToString()!.Contains("开始")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => state.ToString()!.Contains("完成")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Handle 应调用 next 委托并返回其结果
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCallNextAndReturnResponse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleLoggingBehavior<TestRequest, TestResponse>>>();
        var behavior = new NoelleLoggingBehavior<TestRequest, TestResponse>(mockLogger.Object);
        var request = new TestRequest { Data = "sample" };
        var expectedResponse = new TestResponse { Result = "output" };

        // Act
        var response = await behavior.Handle(request, _ => Task.FromResult(expectedResponse), CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("output", response.Result);
    }

    /// <summary>
    /// 当 next 委托抛出异常时，异常应向上传播
    /// </summary>
    [Fact]
    public async Task Handle_WhenNextThrows_ShouldPropagateException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleLoggingBehavior<TestRequest, TestResponse>>>();
        var behavior = new NoelleLoggingBehavior<TestRequest, TestResponse>(mockLogger.Object);
        var request = new TestRequest { Data = "error-case" };
        var expectedException = new InvalidOperationException("Test exception");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(request, _ => throw expectedException, CancellationToken.None));

        Assert.Equal("Test exception", exception.Message);
    }

    /// <summary>
    /// Handle 应处理 CancellationToken 被取消的情况
    /// </summary>
    [Fact]
    public async Task Handle_WithCancelledToken_ShouldPropagateCancellation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleLoggingBehavior<TestRequest, TestResponse>>>();
        var behavior = new NoelleLoggingBehavior<TestRequest, TestResponse>(mockLogger.Object);
        var request = new TestRequest { Data = "cancel-test" };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => behavior.Handle(request, _ => throw new OperationCanceledException(cts.Token), cts.Token));
    }

    #endregion
}

#region Test Types

/// <summary>
/// 测试请求
/// </summary>
public class TestRequest
{
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// 测试响应
/// </summary>
public class TestResponse
{
    public string Result { get; set; } = string.Empty;
}

#endregion
