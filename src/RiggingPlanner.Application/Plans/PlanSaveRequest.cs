using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Application.Plans;

public sealed record PlanSaveRequest
{
    public Guid? PlanId { get; init; }
    public bool IsDraftPlan { get; init; }
    public PlanFormModel Form { get; init; } = new();
    public RiggingPlan? CapacitySource { get; init; }
    public IReadOnlyList<PlanAccessory> Accessories { get; init; } = [];
    public RiggingPlan? AttachmentSource { get; init; }
    public IReadOnlyList<Equipment> EquipmentOptions { get; init; } = [];
    public IReadOnlyList<Rigger> RiggerOptions { get; init; } = [];
}

public sealed record PlanSaveResult(string? Error, IReadOnlyList<string> ValidationErrors, Guid? PlanId)
{
    public bool Success => Error is null && ValidationErrors.Count == 0;
}
