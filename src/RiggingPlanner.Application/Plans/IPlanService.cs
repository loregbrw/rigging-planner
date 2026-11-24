using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Application.Plans;

public interface IPlanService
{
    Task<PlanSaveResult> SaveAsync(PlanSaveRequest request);
    Task<string?> FinalizeAsync(PlanSaveRequest request, DateTime? finalizedAt = null);
    Task<RiggingPlan?> LoadPlanAsync(Guid planId, bool includeAccessories = false, bool includeAttachments = false);
    Task<IReadOnlyList<PlanAccessory>> GetAccessoriesAsync(Guid planId);
    Task<IReadOnlyList<RiggingPlan>> ListPlansAsync();
}
