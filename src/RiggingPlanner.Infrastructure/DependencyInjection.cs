using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();

        // Factory (short-lived contexts) because a Blazor Hybrid scope lives as long as the app.
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));

        return services;
    }

    public static void ApplyMigrations(this IServiceProvider services)
    {
        var factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();

        using var context = factory.CreateDbContext();
        context.Database.Migrate();
    }
}
