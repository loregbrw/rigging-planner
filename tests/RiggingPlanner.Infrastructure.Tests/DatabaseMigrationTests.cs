using Microsoft.EntityFrameworkCore;
using RiggingPlanner.Domain.Entities;
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

    [Fact]
    public void Migrate_CreatesExpectedSchema_AndPersistsRelationships()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath};Pooling=False")
            .Options;

        using (var context = new AppDbContext(options))
        {
            context.Database.Migrate();

            var client = new Client { Name = "Cliente de teste", Cnpj = "00.000.000/0001-00" };
            var plan = new RiggingPlan
            {
                PlanNumber = "2610001",
                PlanDate = new DateTime(2026, 10, 29),
                Location = "Curitiba",
                OperationName = "Içamento de teste",
                LoadDescription = "Carga de teste",
                ClientCnpj = "00.000.000/0001-00",
                Client = client
            };

            context.RiggingPlans.Add(plan);
            context.SaveChanges();
        }

        using (var context = new AppDbContext(options))
        {
            var plan = context.RiggingPlans.Include(x => x.Client).Single();

            Assert.NotEqual(Guid.Empty, plan.Id);
            Assert.Equal("Cliente de teste", plan.Client!.Name);
            Assert.Equal("00.000.000/0001-00", plan.ClientCnpj);
        }
    }

    [Fact]
    public void Migrate_SimplifiesAccessories_AndPreservesLegacyCategoryAsTag()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath};Pooling=False")
            .Options;

        var accessoryId = Guid.NewGuid();
        using (var context = new AppDbContext(options))
        {
            context.Database.Migrate("20261029225112_InitialCreate");
            context.Database.ExecuteSqlInterpolated($"""
                INSERT INTO Accessories
                    (Id, Category, Description, UnitWeightKg, Status, CreatedAt, UpdatedAt)
                VALUES
                    ({accessoryId}, {"manilha"}, {"Manilha 3 t"}, {12.5m}, {"active"}, {DateTime.Now}, {DateTime.Now});
                """);
            context.Database.Migrate();
        }

        using (var context = new AppDbContext(options))
        {
            var accessory = context.Accessories.Include(x => x.Tag).Single(x => x.Id == accessoryId);

            Assert.Equal("Manilha 3 t", accessory.Name);
            Assert.Equal("manilha", accessory.Tag!.Name);
            Assert.Equal(12.5m, accessory.UnitWeightKg);
        }
    }

    public void Dispose()
    {
        File.Delete(databasePath);
    }
}
