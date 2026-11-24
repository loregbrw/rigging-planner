using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.Infrastructure.Tests;

public sealed class MigrationCompatibilityTests : IDisposable
{
    private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"migration-{Guid.NewGuid():N}.db");

    [Fact]
    public void ApplyMigrations_ReconcilesExistingHistory()
    {
        using (var connection = new SqliteConnection($"Data Source={databasePath}"))
        {
            connection.Open();
            Execute(connection,
                "CREATE TABLE __EFMigrationsHistory (MigrationId TEXT NOT NULL PRIMARY KEY, ProductVersion TEXT NOT NULL)",
                "INSERT INTO __EFMigrationsHistory VALUES ('20250101000000_PreviousSchema', '10.0.13')",
                "CREATE TABLE ExistingData (Value TEXT NOT NULL)",
                "INSERT INTO ExistingData VALUES ('kept')");
        }

        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath};Pooling=False"));
        using var provider = services.BuildServiceProvider();

        provider.ApplyMigrations();

        using (var connection = new SqliteConnection($"Data Source={databasePath}"))
        {
            connection.Open();
            Assert.Equal("kept", Scalar(connection, "SELECT Value FROM ExistingData"));
            using var current = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={databasePath};Pooling=False").Options);
            var migration = Assert.Single(current.Database.GetMigrations());
            Assert.Contains(migration, current.Database.GetAppliedMigrations());
        }
    }

    [Fact]
    public void ApplyMigrations_CreatesSchemaOnFreshDatabase()
    {
        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath};Pooling=False"));
        using var provider = services.BuildServiceProvider();

        provider.ApplyMigrations();

        using var connection = new SqliteConnection($"Data Source={databasePath}");
        connection.Open();
        Assert.NotNull(Scalar(connection, "SELECT name FROM sqlite_master WHERE type='table' AND name='RiggingPlans'"));
    }

    private static void Execute(SqliteConnection connection, params string[] commands)
    {
        foreach (var command in commands)
        {
            using var sql = connection.CreateCommand();
            sql.CommandText = command;
            sql.ExecuteNonQuery();
        }
    }

    private static object? Scalar(SqliteConnection connection, string command)
    {
        using var sql = connection.CreateCommand();
        sql.CommandText = command;
        return sql.ExecuteScalar();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }
    }
}
