using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Domain;

/// <summary>
/// Builds the immutable, already-formatted stamp model of a drawing sheet
/// from a saved plan and its accessories (ordered by SortOrder).
/// </summary>
public static class DrawingSheetStampBuilder
{
    public static DrawingSheetStampModel Build(
        RiggingPlan plan,
        IReadOnlyList<PlanAccessory> accessories,
        int sheetNumber,
        int sheetTotal)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(accessories);

        if (sheetTotal < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sheetTotal), sheetTotal, "The sheet total must be at least 1.");
        }

        if (sheetNumber < 1 || sheetNumber > sheetTotal)
        {
            throw new ArgumentOutOfRangeException(nameof(sheetNumber), sheetNumber, "The sheet number must be between 1 and the sheet total.");
        }

        var slingTotal = PlanTotals.SectionTotal(accessories, PlanTotals.SlingSection);
        var deviceTotal = PlanTotals.SectionTotal(accessories, PlanTotals.DeviceSection);
        var grossWeightKg = PlanTotals.GrossWeightKg(plan.WeightKg, slingTotal, deviceTotal, plan.ExtraLoadKg);
        var grossWeightTons = PlanTotals.GrossWeightTons(grossWeightKg);
        var utilizationPct = PlanTotals.CraneUtilizationPct(grossWeightTons, plan.LoadCapacityT);

        return new DrawingSheetStampModel(
            LoadDescription: PlanValueFormatter.Text(plan.LoadDescription),
            LengthM: PlanValueFormatter.Trim2(plan.LengthM),
            WidthM: PlanValueFormatter.Trim2(plan.WidthM),
            HeightM: PlanValueFormatter.Trim2(plan.HeightM),
            WeightKg: PlanValueFormatter.Trim2(plan.WeightKg),
            GrossWeightTons: PlanValueFormatter.Fixed2(grossWeightTons),
            LoadCapacityT: PlanValueFormatter.Min1Max2(plan.LoadCapacityT),
            CraneUtilizationPct: PlanValueFormatter.Fixed1(utilizationPct),
            FootingForceKgf: PlanValueFormatter.Trim2(plan.FootingForceKgf),
            MaxWindSpeedMs: PlanValueFormatter.Min1Max2(plan.MaxWindSpeedMs),
            OperationName: PlanValueFormatter.Text(plan.OperationName),
            ClientName: PlanValueFormatter.Text(plan.ClientName),
            ArtNumber: PlanValueFormatter.Text(plan.ArtNumber),
            PlanNumber: plan.PlanNumber,
            RiggerName: PlanValueFormatter.Text(plan.RiggerName),
            RiggerLabel: RiggerLabel(plan),
            CreaLabel: CreaLabel(plan),
            SheetLabel: $"FOLHA {sheetNumber} DE {sheetTotal}");
    }

    // RiggerRpp and RiggerCrea hold only the registration numbers.
    // When a number is not filled the fixed prefix is kept and the number prints as "-".
    private static string RiggerLabel(RiggingPlan plan) =>
        string.IsNullOrWhiteSpace(plan.RiggerRpp)
            ? "RIGGER -"
            : $"RIGGER - RPP-{plan.RiggerRpp.Trim()}";

    private static string CreaLabel(RiggingPlan plan) =>
        string.IsNullOrWhiteSpace(plan.RiggerCrea)
            ? "CREA-PR -"
            : $"CREA-PR {plan.RiggerCrea.Trim()}";
}
