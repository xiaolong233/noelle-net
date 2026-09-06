using MediatR;

namespace NoelleNet.Extensions.MediatR;

/// <summary>
/// <see cref="NoelleNoMediator"/> 的契约测试：空实现各方法返回默认值/已完成任务且不抛异常
/// </summary>
public class NoelleNoMediatorTests
{
    /// <summary>
    /// CreateStream 两个重载应返回 null 流
    /// </summary>
    [Fact]
    public void CreateStream_ShouldReturnNull()
    {
        var mediator = new NoelleNoMediator();

        Assert.Null(mediator.CreateStream<string>(new TestStreamRequest()));
        Assert.Null(mediator.CreateStream((object)new TestStreamRequest()));
    }

    /// <summary>
    /// Publish 两个重载应返回已完成任务，null 通知不抛异常
    /// </summary>
    [Fact]
    public async Task Publish_ShouldReturnCompletedTaskWithoutThrow()
    {
        var mediator = new NoelleNoMediator();

        await mediator.Publish(new TestNotification());
        await mediator.Publish<TestNotification>(new TestNotification());
        await mediator.Publish(notification: null!);
    }

    /// <summary>
    /// Send 各重载应返回默认值/已完成任务，null 请求不抛异常
    /// </summary>
    [Fact]
    public async Task Send_ShouldReturnDefaultWithoutThrow()
    {
        var mediator = new NoelleNoMediator();

        Assert.Null(await mediator.Send<string>(new TestRequestWithResponse()));
        await mediator.Send(new TestRequestWithoutResponse());
        Assert.Null(await mediator.Send((object)new TestRequestWithResponse()));
        await mediator.Send(request: null!);
    }
}

public class TestStreamRequest : IStreamRequest<string>;

public class TestNotification : INotification;

public class TestRequestWithResponse : IRequest<string>;

public class TestRequestWithoutResponse : IRequest;
