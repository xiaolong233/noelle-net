using Microsoft.EntityFrameworkCore;
using Noelle.Todo.Entities;

namespace Noelle.Todo.Data;

/// <summary>
/// 数据库上下文
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
}
