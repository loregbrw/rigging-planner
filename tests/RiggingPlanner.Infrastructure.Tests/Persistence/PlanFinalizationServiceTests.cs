using Microsoft.EntityFrameworkCore;
using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.Infrastructure.Tests.Persistence;

public sealed class PlanFinalizationServiceTests : IDisposable
{
    private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
    private readonly DbContextOptions<AppDbContext> options;

    public PlanFinalizationServiceTests()
    {
        options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath};Pooling=False")
            .Options;
    }

    [Fact]
    public async Task Finalize_PersistsStatusFinalizedAtAndSnapshot_AndRejectsSecondAttempt()
    {
        var planId = Guid.NewGuid();
        using (var seed = new AppDbContext(options))
        {
            seed.Database.Migrate();
            seed.RiggingPlans.Add(new RiggingPlan
            {
                Id = planId,
                PlanNumber = "2611901",
                PlanDate = new DateTime(2026, 11, 19),
                Location = "Curitiba",
                OperationName = "Içamento de teste",
                LoadDescription = "Carga de teste",
                Status = "draft"
            });
            await seed.SaveChangesAsync();
        }

        var finalizedAt = new DateTime(2026, 11, 19, 10, 30, 0);
        var service = new PlanFinalizationService();

        string? firstError;
        using (var db = new AppDbContext(options))
        {
            firstError = await service.FinalizeAsync(
                db,
                planId,
                _ => Task.FromResult(new PlanDraftSaveResult(null, planId)),
                finalizedAt);
        }
        Assert.Null(firstError);

        using (var verify = new AppDbContext(options))
        {
            var plan = await verify.RiggingPlans
                .Include(x => x.ProcedureSnapshot)
                .SingleAsync(x => x.Id == planId);
            Assert.Equal("finalized", plan.Status);
            Assert.Equal(finalizedAt, plan.FinalizedAt);
            Assert.NotNull(plan.ProcedureSnapshot);
            Assert.Equal(planId, plan.ProcedureSnapshot!.PlanId);
            Assert.False(string.IsNullOrWhiteSpace(plan.ProcedureSnapshot.Content));
        }

        string? secondError;
        using (var db = new AppDbContext(options))
        {
            secondError = await service.FinalizeAsync(
                db,
                planId,
                _ => Task.FromResult(new PlanDraftSaveResult(null, planId)),
                finalizedAt);
        }
        Assert.Equal("O plano já está finalizado.", secondError);
    }

    public void Dispose()
    {
        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }
    }
}
