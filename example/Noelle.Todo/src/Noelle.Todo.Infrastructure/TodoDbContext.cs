using MediatR;
using Microsoft.EntityFrameworkCore;
using Noelle.Todo.Domain.Todo.Entities;
using NoelleNet.EntityFrameworkCore;
using System.Reflection;

namespace Noelle.Todo.Infrastructure;

public class TodoDbContext(DbContextOptions<TodoDbContext> options, IMediator mediator) : NoelleDbContext(options, mediator)
{
    public DbSet<TodoItem> TodoItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
