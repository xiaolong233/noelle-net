using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Noelle.Todo.Domain.Todo.Entities;
using System.Reflection;

namespace Noelle.Todo.Infrastructure;

public class TodoDbContext(DbContextOptions<TodoDbContext> options) : IdentityDbContext<IdentityUser<long>, IdentityRole<long>, long>(options)
{
    public DbSet<TodoItem> TodoItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
