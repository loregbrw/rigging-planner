using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Domain;

/// <summary>
/// Display-only formulas shared by the UI (CapacityState) and the PDF document builders.
/// Nothing here is ever persisted; rounding happens only at formatting time.
/// </summary>
public static class PlanTotals
{
    public const string SlingSection = "lingada";
    public const string DeviceSection = "dispositivo";

    public static decimal AccessoryLineTotal(PlanAccessory item) => item.Quantity * item.UnitWeightKg;

    public static decimal SectionTotal(IEnumerable<PlanAccessory> items, string section) =>
        items.Where(x => x.Section == section).Sum(AccessoryLineTotal);

    public static decimal GrossWeightKg(decimal? netWeightKg, decimal slingTotal, decimal deviceTotal, decimal? extraLoadKg) =>
        (netWeightKg ?? 0) + slingTotal + deviceTotal + (extraLoadKg ?? 0);

    public static decimal GrossWeightTons(decimal grossWeightKg) => grossWeightKg / 1000m;

    public static decimal? CraneUtilizationPct(decimal grossWeightTons, decimal? loadCapacityT) =>
        loadCapacityT is > 0 ? grossWeightTons / loadCapacityT.Value * 100 : null;
}
