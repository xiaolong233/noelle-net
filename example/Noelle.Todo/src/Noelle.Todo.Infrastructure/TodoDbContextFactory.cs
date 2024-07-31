using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NoelleNet.Extensions.MediatR;

namespace Noelle.Todo.Infrastructure;

public class TodoDbContextFactory : IDesignTimeDbContextFactory<TodoDbContext>
{
    public TodoDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder().SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Noelle.Todo.Migratior"))
                                               .AddJsonFile("appsettings.json")
                                               .AddEnvironmentVariables()
                                               .Build();
        string connectionString = config.GetRequiredConnectionString("Default");
        string versionString = config.GetRequiredValue("MySql:Version");
        var serverVersion = new MySqlServerVersion(new Version(versionString));

        DbContextOptionsBuilder<TodoDbContext> builder = new();
        builder.UseMySql(connectionString, serverVersion);

        NoelleNoMediator mediator = new NoelleNoMediator(); 

        return new TodoDbContext(builder.Options, mediator);
    }
}
