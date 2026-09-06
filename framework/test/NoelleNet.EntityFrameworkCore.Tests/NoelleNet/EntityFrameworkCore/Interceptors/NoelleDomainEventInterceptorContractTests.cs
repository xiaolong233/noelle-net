using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NoelleNet.Ddd.Domain.Events;
using NoelleNet.EventBus.Abstractions.Local;

namespace NoelleNet.EntityFrameworkCore.Interceptors;

/// <summary>
/// <see cref="NoelleDomainEventInterceptor"/> 的契约测试（基于 SQLite 验证真实的 SQL 生成行为）。
/// 契约：领域事件在 SaveChanges 提交前派发，处理器对实体的改动会被本次保存捕获并持久化；
/// 处理器内禁止嵌套调用 SaveChanges。
/// </summary>
public class NoelleDomainEventInterceptorContractTests
{
    /// <summary>
    /// 处理器修改已跟踪实体的属性时，改动应被本次 SaveChangesAsync 捕获并持久化
    /// </summary>
    [Fact]
    public async Task SavingChangesAsync_HandlerMutatesTrackedEntity_ShouldBePersisted()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        var context = CreateContext(connection, new TestLocalEventBus(async (eventData, _) =>
        {
            if (eventData is TitleChangedContractEvent e)
                e.Entity.Title = e.NewTitle;   // 修改已跟踪实体（SaveChanges 前实体状态为 Unchanged）
            await Task.CompletedTask;
        }));

        var entity = new ContractEntity { Title = "original" };
        context.ContractEntities.Add(entity);
        await context.SaveChangesAsync();

        // 实体此刻已持久化且状态为 Unchanged；仅添加领域事件
        entity.AddDomainEvent(new TitleChangedContractEvent(entity, "modified-by-handler"));

        // Act
        await context.SaveChangesAsync();

        // Assert：处理器在提交前修改的属性被持久化（DetectChanges 捕获了拦截器期间的改动）
        await using var verifyContext = CreateContext(connection, new TestLocalEventBus());
        var persisted = await verifyContext.ContractEntities.AsNoTracking().SingleAsync();
        Assert.Equal("modified-by-handler", persisted.Title);
    }

    /// <summary>
    /// 处理器新增实体时，新增的实体应被本次 SaveChangesAsync 一并保存
    /// </summary>
    [Fact]
    public async Task SavingChangesAsync_HandlerAddsEntity_ShouldBeSavedInSameSaveChanges()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        ContractDbContext context = null!;
        context = CreateContext(connection, new TestLocalEventBus(async (_, _) =>
        {
            context.ContractEntities.Add(new ContractEntity { Title = "added-by-handler" });
            await Task.CompletedTask;
        }));

        var entity = new ContractEntity { Title = "original" };
        context.ContractEntities.Add(entity);
        entity.AddDomainEvent(new TitleChangedContractEvent(entity, "original"));

        // Act
        await context.SaveChangesAsync();

        // Assert：处理器新增的实体与源实体在同一次保存中持久化
        var titles = await context.ContractEntities.AsNoTracking().Select(x => x.Title).ToListAsync();
        Assert.Contains("original", titles);
        Assert.Contains("added-by-handler", titles);
    }

    /// <summary>
    /// 打开 SQLite 内存连接
    /// </summary>
    private static async Task<SqliteConnection> OpenConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        return connection;
    }

    /// <summary>
    /// 创建带领域事件拦截器的 DbContext
    /// </summary>
    private static ContractDbContext CreateContext(SqliteConnection connection, ILocalEventBus eventBus)
    {
        var options = new DbContextOptionsBuilder<ContractDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new ContractDbContext(options, new NoelleDomainEventInterceptor(eventBus));
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// 测试用本地事件总线：把事件转发给指定的处理器
    /// </summary>
    private sealed class TestLocalEventBus(Func<object, CancellationToken, Task>? handler = null) : ILocalEventBus
    {
        public Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
        {
            if (handler != null && eventData != null)
                return handler(eventData, cancellationToken);

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 契约测试用实体
    /// </summary>
    public class ContractEntity : IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        public int Id { get; set; }
        public string Title { get; set; } = "";

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

        public void ClearDomainEvents() => _domainEvents.Clear();
    }

    /// <summary>
    /// 契约测试用领域事件
    /// </summary>
    public record TitleChangedContractEvent(ContractEntity Entity, string NewTitle) : IDomainEvent;

    /// <summary>
    /// 契约测试用 DbContext
    /// </summary>
    public class ContractDbContext(DbContextOptions<ContractDbContext> options, NoelleDomainEventInterceptor interceptor) : DbContext(options)
    {
        public DbSet<ContractEntity> ContractEntities => Set<ContractEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(interceptor);
        }
    }
}
