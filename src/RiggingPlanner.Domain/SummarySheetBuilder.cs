using System.Globalization;
using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Domain;

/// <summary>
/// Builds the immutable, already-formatted model of the summary sheet
/// from a saved plan and its accessories (ordered by SortOrder).
/// </summary>
public static class SummarySheetBuilder
{
    private static readonly CultureInfo PtBrCulture = new("pt-BR");

    public static SummarySheetModel Build(RiggingPlan plan, IReadOnlyList<PlanAccessory> accessories)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(accessories);

        var slingLines = accessories
            .Where(x => x.Section == PlanTotals.SlingSection)
            .OrderBy(x => x.SortOrder)
            .Select((item, index) => ToLine(index + 1, item, showDrawingRef: false))
            .ToList();

        var slingTotal = PlanTotals.SectionTotal(accessories, PlanTotals.SlingSection);

        var deviceLines = accessories
            .Where(x => x.Section == PlanTotals.DeviceSection)
            .OrderBy(x => x.SortOrder)
            .Select((item, index) => ToLine(slingLines.Count + index + 1, item, showDrawingRef: true))
            .ToList();

        var deviceTotal = PlanTotals.SectionTotal(accessories, PlanTotals.DeviceSection);

        var grossWeightKg = PlanTotals.GrossWeightKg(plan.WeightKg, slingTotal, deviceTotal, plan.ExtraLoadKg);
        var grossWeightTons = PlanTotals.GrossWeightTons(grossWeightKg);
        var utilizationPct = PlanTotals.CraneUtilizationPct(grossWeightTons, plan.LoadCapacityT);

        return new SummarySheetModel(
            ClientName: PlanValueFormatter.Text(plan.ClientName),
            Location: PlanValueFormatter.Text(plan.Location),
            PlanDate: PlanValueFormatter.Date((DateTime?)plan.PlanDate),
            PlanNumber: plan.PlanNumber,
            LoadDescription: PlanValueFormatter.Text(plan.LoadDescription),
            LengthM: PlanValueFormatter.Trim2(plan.LengthM),
            WidthM: PlanValueFormatter.Trim2(plan.WidthM),
            HeightM: PlanValueFormatter.Trim2(plan.HeightM),
            WeightKg: PlanValueFormatter.Trim2(plan.WeightKg),
            SlingLines: slingLines,
            SlingTotal: PlanValueFormatter.Trim2(slingTotal),
            DeviceLines: deviceLines,
            DeviceTotal: PlanValueFormatter.Trim2(deviceTotal),
            CraneModel: CraneModelDisplay(plan),
            TruckModel: PlanValueFormatter.Text(plan.TruckModel),
            LicensePlate: PlanValueFormatter.Text(plan.LicensePlate),
            ChassisNumber: PlanValueFormatter.Text(plan.ChassisNumber),
            ManufactureYear: FormatYear(plan.ManufactureYear),
            CraneNominalCapacity: PlanValueFormatter.Min1Max2(plan.CraneNominalCapacity),
            BoomLengthM: PlanValueFormatter.Min1Max2(plan.BoomLengthM),
            OperatingRadiusM: PlanValueFormatter.Min1Max2(plan.OperatingRadiusM),
            LoadCapacityT: PlanValueFormatter.Min1Max2(plan.LoadCapacityT),
            NetWeightKg: PlanValueFormatter.Trim2(plan.WeightKg),
            SlingWeightKg: PlanValueFormatter.Trim2(slingTotal),
            DeviceWeightKg: PlanValueFormatter.Trim2(deviceTotal),
            ExtraLoadKg: PlanValueFormatter.Trim2(plan.ExtraLoadKg),
            GrossWeightKg: PlanValueFormatter.Int0(grossWeightKg),
            QuadrantStart: PlanValueFormatter.Text(plan.QuadrantStart),
            QuadrantEnd: PlanValueFormatter.Text(plan.QuadrantEnd),
            GrossWeightTons: PlanValueFormatter.Fixed2(grossWeightTons),
            CraneLoadCapacityT: PlanValueFormatter.Min1Max2(plan.LoadCapacityT),
            CraneUtilizationPct: PlanValueFormatter.Fixed1(utilizationPct),
            FlatStrapUtilizationPct: PlanValueFormatter.Min1Max2(plan.FlatStrapUtilizationPct),
            TubularStrapUtilizationPct: PlanValueFormatter.Min1Max2(plan.TubularStrapUtilizationPct),
            WireRopeSlingUtilizationPct: PlanValueFormatter.Min1Max2(plan.WireRopeSlingUtilizationPct),
            ShackleUtilizationPct: PlanValueFormatter.Min1Max2(plan.ShackleUtilizationPct),
            FootingForceKgf: PlanValueFormatter.Trim2(plan.FootingForceKgf),
            MinGroundResistanceKgfCm2: PlanValueFormatter.Trim2(plan.MinGroundResistanceKgfCm2),
            MaxWindSpeedMs: PlanValueFormatter.Min1Max2(plan.MaxWindSpeedMs),
            ArtDisplay: ArtDisplay(plan.ArtNumber),
            Observations: plan.Observations ?? string.Empty,
            RiggerSignature: RiggerSignature(plan));
    }

    private static SummarySheetAccessoryLine ToLine(int number, PlanAccessory item, bool showDrawingRef) =>
        new(
            Number: number,
            Description: PlanValueFormatter.Text(item.Description),
            DrawingRef: showDrawingRef ? PlanValueFormatter.Text(item.DrawingRef) : string.Empty,
            Quantity: PlanValueFormatter.Trim2(item.Quantity),
            UnitWeightKg: PlanValueFormatter.Trim2(item.UnitWeightKg),
            LineTotal: PlanValueFormatter.Trim2(PlanTotals.AccessoryLineTotal(item)));

    private static string FormatYear(int? year) =>
        year is null ? "-" : year.Value.ToString(CultureInfo.InvariantCulture);

    // "Modelo" shows brand and model together, ignoring an empty brand part.
    private static string CraneModelDisplay(RiggingPlan plan)
    {
        var brand = plan.CraneBrand?.Trim() ?? string.Empty;
        var model = plan.CraneModel?.Trim() ?? string.Empty;

        if (brand.Length == 0)
        {
            return PlanValueFormatter.Text(model);
        }

        if (model.Length == 0)
        {
            return PlanValueFormatter.Text(brand);
        }

        if (model.StartsWith(brand, StringComparison.OrdinalIgnoreCase))
        {
            return PlanValueFormatter.Text(model);
        }

        return PlanValueFormatter.Text($"{brand} {model}");
    }

    private static string ArtDisplay(string? artNumber)
    {
        var value = PlanValueFormatter.Text(artNumber);
        return value == "-" ? "-" : $"ART {value}";
    }

    // RiggerName holds the display name and RiggerRpp holds only the number.
    // When the RPP is not filled, the " - RPP-..." part is omitted.
    private static string RiggerSignature(RiggingPlan plan)
    {
        var name = PlanValueFormatter.Text(plan.RiggerName);
        return string.IsNullOrWhiteSpace(plan.RiggerRpp)
            ? $"Rigger {name}"
            : $"Rigger {name} - RPP-{plan.RiggerRpp.Trim()}";
    }
}
