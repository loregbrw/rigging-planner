namespace RiggingPlanner.Domain;

/// <summary>
/// One row of the accessory tables ("LINGADA / AMARRAS" or "DISPOSITIVOS / BALANCIM").
/// All strings are already formatted for printing.
/// </summary>
public sealed record SummarySheetAccessoryLine(
    int Number,
    string Description,
    string DrawingRef,
    string Quantity,
    string UnitWeightKg,
    string LineTotal);

/// <summary>
/// Immutable model of the summary sheet (page 1, "Capacidade de carga").
/// Every string is final; the renderer only positions text.
/// </summary>
public sealed record SummarySheetModel(
    string ClientName,
    string Location,
    string PlanDate,
    string PlanNumber,
    string LoadDescription,
    string LengthM,
    string WidthM,
    string HeightM,
    string WeightKg,
    IReadOnlyList<SummarySheetAccessoryLine> SlingLines,
    string SlingTotal,
    IReadOnlyList<SummarySheetAccessoryLine> DeviceLines,
    string DeviceTotal,
    string CraneModel,
    string TruckModel,
    string LicensePlate,
    string ChassisNumber,
    string ManufactureYear,
    string CraneNominalCapacity,
    string BoomLengthM,
    string OperatingRadiusM,
    string LoadCapacityT,
    string NetWeightKg,
    string SlingWeightKg,
    string DeviceWeightKg,
    string ExtraLoadKg,
    string GrossWeightKg,
    string QuadrantStart,
    string QuadrantEnd,
    string GrossWeightTons,
    string CraneLoadCapacityT,
    string CraneUtilizationPct,
    string FlatStrapUtilizationPct,
    string TubularStrapUtilizationPct,
    string WireRopeSlingUtilizationPct,
    string ShackleUtilizationPct,
    string FootingForceKgf,
    string MinGroundResistanceKgfCm2,
    string MaxWindSpeedMs,
    string ArtDisplay,
    string Observations,
    string RiggerSignature);
