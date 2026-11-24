using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiggingPlanner.Application.Abstractions;
using RiggingPlanner.Application.Plans;
using RiggingPlanner.Infrastructure.Persistence;
using RiggingPlanner.Infrastructure.Plans;
using RiggingPlanner.Infrastructure.Storage;

namespace RiggingPlanner.Infrastructure.Tests.Application;

public sealed class PlanServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"plan-service-{Guid.NewGuid():N}");

    [Fact]
    public async Task SaveAsync_CreatesPlanWithMonthlyNumber()
    {
        var service = CreateService();

        var result = await service.SaveAsync(new PlanSaveRequest
        {
            Form = new PlanFormModel
            {
                OperationName = "Içamento de teste",
                Location = "Curitiba",
                PlanDate = new DateTime(2026, 7, 3),
                ClientName = "Cliente de teste",
                ClientCnpj = "00.000.000/0000-00",
                LoadDescription = "Carga de teste",
                EquipmentModel = "Modelo de teste",
                EquipmentTruckModel = "Caminhão de teste",
                EquipmentChassisNumber = "CHASSIS-TESTE",
                EquipmentLicensePlate = "AAA-0000",
                RiggerDisplayName = "Responsável de teste",
                RiggerFullName = "Responsável de teste",
                RiggerRppNumber = "123"
            }
        });

        Assert.True(result.Success, string.Join(" ", result.ValidationErrors));
        var plan = await service.LoadPlanAsync(result.PlanId!.Value, includeAccessories: true);
        Assert.NotNull(plan);
        Assert.Equal("2607001", plan.PlanNumber);
        Assert.Equal("draft", plan.Status);
    }

    [Fact]
    public async Task SaveAsync_RejectsInvalidFormWithoutTouchingTheDatabase()
    {
        var service = CreateService();

        var result = await service.SaveAsync(new PlanSaveRequest());

        Assert.False(result.Success);
        Assert.NotEmpty(result.ValidationErrors);
        Assert.Empty(await service.ListPlansAsync());
    }

    private IPlanService CreateService()
    {
        Directory.CreateDirectory(root);
        var databasePath = Path.Combine(root, "rigging-planner.db");

        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath};Pooling=False"));
        services.AddSingleton<IFileStore>(new FileStore(databasePath));
        services.AddScoped<IPlanService, PlanService>();

        var provider = services.BuildServiceProvider();
        using (var scope = provider.CreateScope())
        {
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var context = factory.CreateDbContext();
            context.Database.Migrate();
        }

        return provider.CreateScope().ServiceProvider.GetRequiredService<IPlanService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
