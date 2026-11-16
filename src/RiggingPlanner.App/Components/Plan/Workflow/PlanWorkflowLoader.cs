using Microsoft.EntityFrameworkCore;
using RiggingPlanner.App.Components.Shared;
using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.App.Components.Plan.Workflow;

public static class PlanWorkflowLoader
{
    public static async Task<RiggingPlan?> LoadAsync(
        AppDbContext db,
        PlanDraftState draft,
        Guid planId,
        bool includeAccessories = false,
        bool useDraftFallback = false)
    {
        if (draft.PlanId == planId && draft.Plan is not null && draft.IsInitialized)
        {
            if (includeAccessories && draft.IsPersisted && draft.Accessories.Count == 0)
            {
                await LoadAccessoriesAsync(db, draft, planId);
            }

            return draft.Plan;
        }

        var plan = await db.RiggingPlans
            .AsNoTracking()
            .Include(x => x.ArtFile)
            .Include(x => x.Drawings)
            .ThenInclude(x => x.File)
            .SingleOrDefaultAsync(x => x.Id == planId && x.DeletedAt == null);

        if (plan is not null)
        {
            draft.Plan = plan;
            draft.PlanId = planId;
            draft.IsPersisted = true;
            draft.IsInitialized = true;

            if (includeAccessories)
            {
                await LoadAccessoriesAsync(db, draft, planId);
            }

            return plan;
        }

        if (useDraftFallback && draft.PlanId == planId && draft.Plan is not null)
        {
            return draft.Plan;
        }

        return null;
    }

    private static async Task LoadAccessoriesAsync(AppDbContext db, PlanDraftState draft, Guid planId)
    {
        var accessories = await db.PlanAccessories
            .Where(x => x.PlanId == planId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        draft.Accessories.Clear();
        draft.Accessories.AddRange(accessories);
    }
}
