using NoelleNet.EventBus.Abstractions.Local;

namespace NoelleNet.Ddd.Domain.Events;

/// <summary>
/// 领域事件处理程序接口
/// </summary>
/// <remarks>
/// 领域事件由 <c>NoelleDomainEventInterceptor</c> 在 <c>SaveChanges</c> 提交前派发，处理器在数据库事务内执行。约束如下：
/// <list type="bullet">
/// <item>处理器可以新增、修改或删除实体，改动会被本次 <c>SaveChanges</c> 捕获并随事务一起提交；</item>
/// <item>处理器内禁止再次调用 <c>SaveChanges</c>（同一 DbContext 嵌套保存会导致重复提交与状态不一致，EF Core 不保证此场景的正确性）；</item>
/// <item>短信、HTTP 调用、缓存等外部副作用请通过 <c>IDistributedEventBus</c> 发布集成事件（配合事务发件箱保证一致性），不要在处理器内直接执行；</item>
/// <item>同步 <c>SaveChanges</c> 路径会阻塞等待事件处理完成，推荐使用异步 <c>SaveChangesAsync</c>。</item>
/// </list>
/// </remarks>
/// <typeparam name="TEvent">领域事件类型</typeparam>
public interface IDomainEventHandler<in TEvent> : ILocalEventHandler<TEvent> where TEvent : IDomainEvent
{
}
