using MediatR;

namespace NoelleNet.Extensions.MediatR;

/// <summary>
/// <see cref="NoelleNoMediator"/> 的单元测试
/// </summary>
public class NoelleNoMediatorTests
{
    #region Constructor

    /// <summary>
    /// 创建实例时应成功返回非空对象
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act
        var mediator = new NoelleNoMediator();

        // Assert
        Assert.NotNull(mediator);
    }

    #endregion

    #region Interface Implementation

    /// <summary>
    /// NoelleNoMediator 应实现 IMediator 接口
    /// </summary>
    [Fact]
    public void ShouldImplementIMediator()
    {
        // Act
        var mediator = new NoelleNoMediator();

        // Assert
        Assert.IsAssignableFrom<IMediator>(mediator);
    }

    #endregion

    #region CreateStream

    /// <summary>
    /// CreateStream&lt;TResponse&gt; 应返回默认值
    /// </summary>
    [Fact]
    public void CreateStream_Generic_ShouldReturnDefault()
    {
        // Arrange
        var mediator = new NoelleNoMediator();
        var request = new TestStreamRequest();

        // Act
        var result = mediator.CreateStream<string>(request);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// CreateStream(object) 应返回默认值
    /// </summary>
    [Fact]
    public void CreateStream_Object_ShouldReturnDefault()
    {
        // Arrange
        var mediator = new NoelleNoMediator();
        object request = new TestStreamRequest();

        // Act
        var result = mediator.CreateStream(request);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Publish

    /// <summary>
    /// Publish(object) 应返回已完成的任务
    /// </summary>
    [Fact]
    public async Task Publish_Object_ShouldReturnCompletedTask()
    {
        // Arrange
        var mediator = new NoelleNoMediator();
        object notification = new TestNotification();

        // Act
        var task = mediator.Publish(notification);

        // Assert
        await task;
        Assert.True(task.IsCompletedSuccessfully);
    }

    /// <summary>
    /// Publish&lt;TNotification&gt; 应返回已完成的任务
    /// </summary>
    [Fact]
    public async Task Publish_Generic_ShouldReturnCompletedTask()
    {
        // Arrange
        var mediator = new NoelleNoMediator();
        var notification = new TestNotification();

        // Act
        var task = mediator.Publish(notification);

        // Assert
        await task;
        Assert.True(task.IsCompletedSuccessfully);
    }

    /// <summary>
    /// Publish 传入 null 通知不应抛出异常
    /// </summary>
    [Fact]
    public async Task Publish_WithNullNotification_ShouldNotThrow()
    {
        // Arrange
        var mediator = new NoelleNoMediator();

        // Act & Assert
        var exception = await Record.ExceptionAsync(
            () => mediator.Publish(notification: null!));
        Assert.Null(exception);
    }

    #endregion

    #region Send

    /// <summary>
    /// Send&lt;TResponse&gt;(IRequest&lt;TResponse&gt;) 应返回默认值
    /// </summary>
    [Fact]
    public async Task Send_GenericRequestWithResponse_ShouldReturnDefault()
    {
        // Arrange
        var mediator = new NoelleNoMediator();
        var request = new TestRequestWithResponse();

        // Act
        var result = await mediator.Send<string>(request);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Send&lt;TRequest&gt;(TRequest) where TRequest : IRequest 应返回已完成的任务
    /// </summary>
    [Fact]
    public async Task Send_GenericRequestWithoutResponse_ShouldReturnCompletedTask()
    {
        // Arrange
        var mediator = new NoelleNoMediator();
        var request = new TestRequestWithoutResponse();

        // Act
        var task = mediator.Send(request);

        // Assert
        await task;
        Assert.True(task.IsCompletedSuccessfully);
    }

    /// <summary>
    /// Send(object) 应返回 null
    /// </summary>
    [Fact]
    public async Task Send_Object_ShouldReturnNull()
    {
        // Arrange
        var mediator = new NoelleNoMediator();
        object request = new TestRequestWithResponse();

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Send 传入 null 请求不应抛出异常
    /// </summary>
    [Fact]
    public async Task Send_WithNullRequest_ShouldNotThrow()
    {
        // Arrange
        var mediator = new NoelleNoMediator();

        // Act & Assert
        var exception = await Record.ExceptionAsync(
            () => mediator.Send(request: null!));
        Assert.Null(exception);
    }

    #endregion
}

#region Test Types

/// <summary>
/// 测试用 Stream 请求
/// </summary>
public class TestStreamRequest : IStreamRequest<string>
{
}

/// <summary>
/// 测试用通知
/// </summary>
public class TestNotification : INotification
{
}

/// <summary>
/// 测试用带响应的请求
/// </summary>
public class TestRequestWithResponse : IRequest<string>
{
}

/// <summary>
/// 测试用不带响应的请求
/// </summary>
public class TestRequestWithoutResponse : IRequest
{
}

#endregion
