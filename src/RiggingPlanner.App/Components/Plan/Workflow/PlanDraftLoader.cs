using RiggingPlanner.Application.Plans;
using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.App.Components.Plan.Workflow;

public static class PlanDraftLoader
{
    public static async Task<RiggingPlan?> LoadAsync(
        IPlanService plans,
        PlanDraftState draft,
        Guid planId,
        bool includeAccessories = false,
        bool useDraftFallback = false)
    {
        if (draft.PlanId == planId && draft.Plan is not null && draft.IsInitialized)
        {
            if (includeAccessories && draft.IsPersisted && draft.Accessories.Count == 0)
            {
                await LoadAccessoriesAsync(plans, draft, planId);
            }

            return draft.Plan;
        }

        var plan = await plans.LoadPlanAsync(planId, includeAccessories, includeAttachments: true);
        if (plan is not null)
        {
            draft.Attachments.Reset();
            draft.Plan = plan;
            draft.PlanId = planId;
            draft.IsPersisted = true;
            draft.IsInitialized = true;

            if (includeAccessories)
            {
                await LoadAccessoriesAsync(plans, draft, planId);
            }

            return plan;
        }

        if (useDraftFallback && draft.PlanId == planId && draft.Plan is not null)
        {
            return draft.Plan;
        }

        return null;
    }

    private static async Task LoadAccessoriesAsync(IPlanService plans, PlanDraftState draft, Guid planId)
    {
        var accessories = await plans.GetAccessoriesAsync(planId);
        draft.Accessories.Clear();
        draft.Accessories.AddRange(accessories);
    }
}
