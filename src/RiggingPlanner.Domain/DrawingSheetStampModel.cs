namespace RiggingPlanner.Domain;

/// <summary>
/// Immutable model of the drawing sheet stamp ("carimbo"). Every string is final;
/// the renderer only positions text. Sheet numbering is part of the model because the
/// stamp repeats on every sheet and only "FOLHA X DE Y" changes.
/// </summary>
public sealed record DrawingSheetStampModel(
    string LoadDescription,
    string LengthM,
    string WidthM,
    string HeightM,
    string WeightKg,
    string GrossWeightTons,
    string LoadCapacityT,
    string CraneUtilizationPct,
    string FootingForceKgf,
    string MaxWindSpeedMs,
    string OperationName,
    string ClientName,
    string ArtNumber,
    string PlanNumber,
    string RiggerName,
    string RiggerLabel,
    string CreaLabel,
    string SheetLabel);
