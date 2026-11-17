using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Infrastructure.Tests.Fixtures;

/// <summary>
/// Reference plan mirroring the example document (Fixtures/reference-plan-2607036.pdf).
/// Values were extracted letter by letter from the example PDF (scratch runs dump).
/// </summary>
public static class TestPlans
{
    public static (RiggingPlan Plan, List<PlanAccessory> Accessories) Reference2607036()
    {
        var plan = new RiggingPlan
        {
            PlanNumber = "2607036",
            PlanDate = new DateTime(2026, 7, 24),
            Location = "Curitiba",
            OperationName = "Chiller",
            ClientName = "Kaefer Rip Serviços Industriais Ltda.",
            LoadDescription = "Chiller",
            LengthM = 4.7m,
            WidthM = 1.95m,
            HeightM = 2.3m,
            WeightKg = 3450m,
            ExtraLoadKg = null,
            TruckModel = "Volvo/VM 270 6X2R",
            LicensePlate = "IUL8A16",
            ChassisNumber = "93KP0R1C7DE141594",
            ManufactureYear = 2013,
            CraneBrand = "Palfinger",
            CraneModel = "MD 45.007",
            CraneNominalCapacity = 45.0m,
            BoomLengthM = 5.1m,
            OperatingRadiusM = 5.0m,
            LoadCapacityT = 7.95m,
            QuadrantStart = "LATERAL",
            QuadrantEnd = "LATERAL",
            FlatStrapUtilizationPct = 60.0m,
            TubularStrapUtilizationPct = null,
            WireRopeSlingUtilizationPct = null,
            ShackleUtilizationPct = 70.0m,
            FootingForceKgf = 7914m,
            MinGroundResistanceKgfCm2 = 3.17m,
            MaxWindSpeedMs = 7.0m,
            RiggerName = "Giuliano Breda",
            RiggerRpp = "23-1040",
            RiggerCrea = "78755/D",
            SupervisorName = null,
            ArtNumber = "1720264496217",
            Observations = string.Empty,
        };

        var accessories = new List<PlanAccessory>
        {
            Sling(1, "Cinta plana 3 metros de comprimento - Capacidade 6t", 4m, 4.87m),
            Sling(2, "Manilha Curva 7/8\"com pino roscado - GRAU 6 - FS 6:1", 6m, 1.7m),
            Sling(3, "Cinta plana 4 metros de comprimento - Capacidade 6t", 2m, 5.97m),
            Sling(4, "Manilha Curva 1\"com pino roscado - GRAU 6 - FS 6:1", 2m, 2.5m),
            Device(5, "Balancim", "-", 1m, 250m),
        };

        return (plan, accessories);
    }

    private static PlanAccessory Sling(int order, string description, decimal quantity, decimal unitWeightKg) =>
        new() { Section = "lingada", Description = description, Quantity = quantity, UnitWeightKg = unitWeightKg, SortOrder = order };

    private static PlanAccessory Device(int order, string description, string drawingRef, decimal quantity, decimal unitWeightKg) =>
        new() { Section = "dispositivo", Description = description, DrawingRef = drawingRef, Quantity = quantity, UnitWeightKg = unitWeightKg, SortOrder = order };
}
