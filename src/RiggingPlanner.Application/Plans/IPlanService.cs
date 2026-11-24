using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Application.Plans;

public sealed record PlanFinalizeResult(string? Error, Guid? PlanId, IReadOnlyList<string> ValidationErrors);

public interface IPlanService
{
    Task<PlanSaveResult> SaveAsync(PlanSaveRequest request);
    Task<PlanFinalizeResult> FinalizeAsync(PlanSaveRequest request, DateTime? finalizedAt = null);
    Task<RiggingPlan?> LoadPlanAsync(Guid planId, bool includeAccessories = false, bool includeAttachments = false);
    Task<IReadOnlyList<PlanAccessory>> GetAccessoriesAsync(Guid planId);
    Task<IReadOnlyList<RiggingPlan>> ListPlansAsync();
}
