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
    public void PersistsCraneNominalCapacity()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath};Pooling=False")
            .Options;

        using (var context = new AppDbContext(options))
        {
            context.Database.Migrate();

            var plan = new RiggingPlan
            {
                PlanNumber = "2611002",
                PlanDate = new DateTime(2026, 11, 10),
                Location = "Curitiba",
                OperationName = "Teste Capacidade Nominal",
                LoadDescription = "Carga",
                CraneNominalCapacity = 75.50m
            };

            context.RiggingPlans.Add(plan);
            context.SaveChanges();
        }

        using (var context = new AppDbContext(options))
        {
            var plan = context.RiggingPlans.Single(x => x.PlanNumber == "2611002");
            Assert.Equal(75.50m, plan.CraneNominalCapacity);
        }
    }

    [Fact]
    public void PersistsOperationalQuadrants()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath};Pooling=False")
            .Options;

        using (var context = new AppDbContext(options))
        {
            context.Database.Migrate();

            var plan = new RiggingPlan
            {
                PlanNumber = "2611003",
                PlanDate = new DateTime(2026, 11, 10),
                Location = "Curitiba",
                OperationName = "Teste Quadrantes",
                LoadDescription = "Carga",
                QuadrantStart = "LATERAL",
                QuadrantEnd = "360 GRAUS"
            };

            context.RiggingPlans.Add(plan);
            context.SaveChanges();
        }

        using (var context = new AppDbContext(options))
        {
            var plan = context.RiggingPlans.Single(x => x.PlanNumber == "2611003");
            Assert.Equal("LATERAL", plan.QuadrantStart);
            Assert.Equal("360 GRAUS", plan.QuadrantEnd);
        }
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
    public void PersistsPlanAccessories_LingadaAndDevice()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath};Pooling=False")
            .Options;

        Guid planId;
        using (var context = new AppDbContext(options))
        {
            context.Database.Migrate();

            var plan = new RiggingPlan
            {
                Id = Guid.NewGuid(),
                PlanNumber = "2611001",
                PlanDate = new DateTime(2026, 11, 10),
                Location = "Curitiba",
                OperationName = "Teste de lingadas e balancim",
                LoadDescription = "Carga pesada"
            };
            planId = plan.Id;

            var lingada = new PlanAccessory
            {
                Id = Guid.NewGuid(),
                PlanId = planId,
                Section = "lingada",
                Description = "Cinta de Poliéster 5t",
                Quantity = 2,
                UnitWeightKg = 15.5m,
                SortOrder = 1
            };

            var device = new PlanAccessory
            {
                Id = Guid.NewGuid(),
                PlanId = planId,
                Section = "dispositivo",
                Description = "Balancim H 3m",
                DrawingRef = "DWG-001",
                Quantity = 1,
                UnitWeightKg = 250m,
                SortOrder = 2
            };

            context.RiggingPlans.Add(plan);
            context.PlanAccessories.Add(lingada);
            context.PlanAccessories.Add(device);
            context.SaveChanges();
        }

        using (var context = new AppDbContext(options))
        {
            var plan = context.RiggingPlans.Include(x => x.Accessories).Single(x => x.Id == planId);
            Assert.Equal(2, plan.Accessories.Count);
            
            var lingada = plan.Accessories.Single(x => x.Section == "lingada");
            Assert.Equal("Cinta de Poliéster 5t", lingada.Description);
            Assert.Equal(2m, lingada.Quantity);
            Assert.Equal(15.5m, lingada.UnitWeightKg);

            var device = plan.Accessories.Single(x => x.Section == "dispositivo");
            Assert.Equal("Balancim H 3m", device.Description);
            Assert.Equal("DWG-001", device.DrawingRef);
            Assert.Equal(1m, device.Quantity);
            Assert.Equal(250m, device.UnitWeightKg);
        }
    }

    public void Dispose()
    {
        File.Delete(databasePath);
    }
}
