using RiggingPlanner.App.Components.Shared;
using RiggingPlanner.Domain;
using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.App.Components.Plan.Capacity;

public sealed class CapacityState
{
    private readonly PlanDraftState draft;

    public CapacityState(PlanDraftState draft)
    {
        this.draft = draft;
        Lingada = new CapacityAccessoryState(this, "lingada", "a lingada", showDrawingRef: false);
        Device = new CapacityAccessoryState(this, "dispositivo", "o dispositivo", showDrawingRef: true);
    }

    public Action? Changed { get; set; }
    public Guid PlanId { get; set; }
    public RiggingPlan? Plan { get; set; }
    public bool IsReadOnly { get; set; }
    public string? ErrorMessage { get; set; }
    public CapacityAccessoryState Lingada { get; }
    public CapacityAccessoryState Device { get; }

    public decimal GrossWeight => PlanTotals.GrossWeightKg(Plan?.WeightKg, Lingada.Total, Device.Total, Plan?.ExtraLoadKg);
    public decimal GrossWeightTons => PlanTotals.GrossWeightTons(GrossWeight);
    public decimal? Utilization => PlanTotals.CraneUtilizationPct(GrossWeightTons, Plan?.LoadCapacityT);

    internal List<PlanAccessory> Items => draft.Accessories;
    internal int NextSortOrder => Items.Count == 0 ? 1 : Items.Max(x => x.SortOrder) + 1;
    internal void NotifyChanged() => Changed?.Invoke();
}
