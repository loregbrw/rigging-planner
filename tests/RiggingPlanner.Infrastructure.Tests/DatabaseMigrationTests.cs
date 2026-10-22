using Microsoft.EntityFrameworkCore;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.Infrastructure.Tests;

public class DatabaseMigrationTests : IDisposable
{
    private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");

    [Fact]
    public void Migrate_OnNewDatabase_AppliesAllMigrations()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath};Pooling=False")
            .Options;

        using var context = new AppDbContext(options);
        context.Database.Migrate();

        Assert.True(context.Database.CanConnect());
        Assert.NotEmpty(context.Database.GetAppliedMigrations());
        Assert.Empty(context.Database.GetPendingMigrations());
    }

    public void Dispose()
    {
        File.Delete(databasePath);
    }
}
