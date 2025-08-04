using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Noelle.Todo.Infrastructure;

public class TodoDbContextFactory : IDesignTimeDbContextFactory<TodoDbContext>
{
    public TodoDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder().SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Noelle.Todo.DbMigratior"))
                                               .AddJsonFile("appsettings.json")
                                               .AddEnvironmentVariables()
                                               .Build();
        string connectionString = config.GetRequiredConnectionString("Default");

        DbContextOptionsBuilder<TodoDbContext> builder = new();
        builder.UseSqlServer(connectionString).UseOpenIddict();

        return new TodoDbContext(builder.Options);
    }
}
