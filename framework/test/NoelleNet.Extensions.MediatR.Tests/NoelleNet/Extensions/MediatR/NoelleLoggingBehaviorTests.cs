using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace NoelleNet.Extensions.MediatR;

/// <summary>
/// <see cref="NoelleLoggingBehavior{TRequest, TResponse}"/> 的契约测试：命令前后各记录一次日志、异常传播
/// </summary>
public class NoelleLoggingBehaviorTests
{
    /// <summary>
    /// 处理前记录"开始"、处理后记录"完成"，并返回 next 的结果
    /// </summary>
    [Fact]
    public async Task Handle_ShouldLogBeforeAndAfter()
    {
        var mockLogger = new Mock<ILogger<NoelleLoggingBehavior<TestRequest, TestResponse>>>();
        var behavior = new NoelleLoggingBehavior<TestRequest, TestResponse>(mockLogger.Object);
        var expectedResponse = new TestResponse { Result = "test-result" };

        var response = await behavior.Handle(
            new TestRequest { Data = "test-data" },
            _ => Task.FromResult(expectedResponse),
            CancellationToken.None);

        Assert.Equal(expectedResponse, response);
        mockLogger.Verify(
            x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("开始")),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        mockLogger.Verify(
            x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("完成")),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// next 抛出的异常（含取消）应向上传播
    /// </summary>
    [Fact]
    public async Task Handle_WhenNextThrows_ShouldPropagate()
    {
        var mockLogger = new Mock<ILogger<NoelleLoggingBehavior<TestRequest, TestResponse>>>();
        var behavior = new NoelleLoggingBehavior<TestRequest, TestResponse>(mockLogger.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(new TestRequest(), _ => throw new InvalidOperationException("Test exception"), CancellationToken.None));
        Assert.Equal("Test exception", exception.Message);

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => behavior.Handle(new TestRequest(), _ => throw new OperationCanceledException(cts.Token), cts.Token));
    }
}

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
