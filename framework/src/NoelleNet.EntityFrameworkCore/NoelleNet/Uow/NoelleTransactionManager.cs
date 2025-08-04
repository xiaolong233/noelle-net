using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace NoelleNet.Uow;

/// <summary>
/// 基于 EF Core 的 <see cref="ITransactionManager"/> 默认实现
/// </summary>
public class NoelleTransactionManager : ITransactionManager
{
    private readonly DbContext _dbContext;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleTransactionManager"/> 实例
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleTransactionManager(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc/>
    public bool HasActiveTransaction => _currentTransaction != null;

    /// <inheritdoc/>
    public Guid? TransactionId => _currentTransaction?.TransactionId;

    /// <inheritdoc/>
    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
            throw new InvalidOperationException();

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task BeginAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
            throw new InvalidOperationException();

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException();

        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();

        _currentTransaction = null;
    }

    /// <inheritdoc/>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException();

        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();

        _currentTransaction = null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed || _currentTransaction == null)
            return;

        _currentTransaction.Dispose();
        _currentTransaction = null;
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed || _currentTransaction == null)
            return;

        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
