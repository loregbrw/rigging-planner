using Microsoft.EntityFrameworkCore;
using RiggingPlanner.Domain.Services;

namespace RiggingPlanner.Infrastructure.Persistence;

public sealed record PlanDraftSaveResult(string? Error, Guid? PlanId);

public sealed class PlanFinalizationService
{
    public async Task<string?> FinalizeAsync(
        AppDbContext db,
        Guid? currentPlanId,
        Func<AppDbContext, Task<PlanDraftSaveResult>> saveDraftAsync,
        DateTime? finalizedAt = null)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        if (currentPlanId is not null)
        {
            var existing = await db.RiggingPlans
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == currentPlanId && x.DeletedAt == null);
            if (existing is null)
            {
                return "Plano não encontrado.";
            }
            if (existing.Status == "finalized")
            {
                return "O plano já está finalizado.";
            }
        }

        var saved = await saveDraftAsync(db);
        if (saved.Error is not null)
        {
            return saved.Error;
        }

        var planId = saved.PlanId ?? currentPlanId;
        if (planId is null)
        {
            return "Plano não encontrado.";
        }

        var plan = await db.RiggingPlans
            .Include(x => x.ProcedureSnapshot)
            .SingleOrDefaultAsync(x => x.Id == planId && x.DeletedAt == null);
        if (plan is null)
        {
            return "Plano não encontrado.";
        }
        if (plan.Status == "finalized")
        {
            return "O plano já está finalizado.";
        }

        var date = finalizedAt ?? DateTime.Now;
        plan.Status = "finalized";
        plan.FinalizedAt = date;
        plan.UpdatedAt = date;

        var snapshot = OperationalProcedureService.CreateSnapshot(plan, date);
        var stored = await db.PlanProcedureSnapshots.SingleOrDefaultAsync(x => x.PlanId == planId);
        if (stored is null)
        {
            db.PlanProcedureSnapshots.Add(snapshot);
        }
        else
        {
            db.Entry(stored).CurrentValues.SetValues(snapshot);
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return null;
    }
}
