using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Noelle.Todo.Domain.Todo.Entities;
using NoelleNet.EntityFrameworkCore;
using System.Reflection;

namespace Noelle.Todo.Infrastructure;

public class TodoDbContext(DbContextOptions<TodoDbContext> options, IMediator mediator) : IdentityDbContext<IdentityUser<long>, IdentityRole<long>, long>(options)
{
    private readonly IMediator _mediator = mediator;

    public DbSet<TodoItem> TodoItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override int SaveChanges()
    {
        _mediator.DispatchDomainEventsAsync(ChangeTracker).Wait();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        _mediator.DispatchDomainEventsAsync(ChangeTracker).Wait();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await _mediator.DispatchDomainEventsAsync(ChangeTracker, cancellationToken);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _mediator.DispatchDomainEventsAsync(ChangeTracker, cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }
}
