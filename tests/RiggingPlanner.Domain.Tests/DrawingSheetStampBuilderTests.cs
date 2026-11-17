using System.Globalization;
using RiggingPlanner.Domain.Entities;
using Xunit;

namespace RiggingPlanner.Domain.Tests;

public class DrawingSheetStampBuilderTests
{
    [Fact]
    public void Build_FormatsEveryStampValue()
    {
        var (plan, accessories) = ReferencePlan();

        var model = RunUnderSystemCultures(() => DrawingSheetStampBuilder.Build(plan, accessories, 1, 3));

        Assert.Equal("Chiller", model.LoadDescription);
        Assert.Equal("4,7", model.LengthM);
        Assert.Equal("1,95", model.WidthM);
        Assert.Equal("2,3", model.HeightM);
        Assert.Equal("3450", model.WeightKg);
        Assert.Equal("3,75", model.GrossWeightTons);
        Assert.Equal("7,95", model.LoadCapacityT);
        Assert.Equal("47,1", model.CraneUtilizationPct);
        Assert.Equal("7914", model.FootingForceKgf);
        Assert.Equal("7,0", model.MaxWindSpeedMs);
        Assert.Equal("Chiller", model.OperationName);
        Assert.Equal("Kaefer Rip Serviços Industriais Ltda.", model.ClientName);
        Assert.Equal("1720264496217", model.ArtNumber);
        Assert.Equal("2607036", model.PlanNumber);
        Assert.Equal("Giuliano Breda", model.RiggerName);
        Assert.Equal("RIGGER - RPP-23-1040", model.RiggerLabel);
        Assert.Equal("CREA-PR 78755/D", model.CreaLabel);
        Assert.Equal("FOLHA 1 DE 3", model.SheetLabel);
    }

    [Fact]
    public void Build_PrintsDashForNullOptionals()
    {
        var (plan, _) = ReferencePlan();
        plan.LoadDescription = string.Empty;
        plan.LengthM = null;
        plan.WidthM = null;
        plan.HeightM = null;
        plan.WeightKg = null;
        plan.ExtraLoadKg = null;
        plan.LoadCapacityT = null;
        plan.FootingForceKgf = null;
        plan.MaxWindSpeedMs = null;
        plan.OperationName = string.Empty;
        plan.ClientName = null;
        plan.ArtNumber = null;
        plan.RiggerName = null;
        plan.RiggerRpp = null;
        plan.RiggerCrea = null;

        var model = RunUnderSystemCultures(() => DrawingSheetStampBuilder.Build(plan, [], 1, 1));

        Assert.Equal("-", model.LoadDescription);
        Assert.Equal("-", model.LengthM);
        Assert.Equal("-", model.WidthM);
        Assert.Equal("-", model.HeightM);
        Assert.Equal("-", model.WeightKg);
        Assert.Equal("0,00", model.GrossWeightTons);
        Assert.Equal("-", model.LoadCapacityT);
        Assert.Equal("-", model.CraneUtilizationPct);
        Assert.Equal("-", model.FootingForceKgf);
        Assert.Equal("-", model.MaxWindSpeedMs);
        Assert.Equal("-", model.OperationName);
        Assert.Equal("-", model.ClientName);
        Assert.Equal("-", model.ArtNumber);
        Assert.Equal("-", model.RiggerName);
        Assert.Equal("RIGGER -", model.RiggerLabel);
        Assert.Equal("CREA-PR -", model.CreaLabel);
        Assert.Equal("FOLHA 1 DE 1", model.SheetLabel);
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(4, 3)]
    [InlineData(1, 0)]
    public void Build_RejectsSheetNumbersOutsideTheRange(int sheetNumber, int sheetTotal)
    {
        var (plan, accessories) = ReferencePlan();

        Assert.Throws<ArgumentOutOfRangeException>(() => DrawingSheetStampBuilder.Build(plan, accessories, sheetNumber, sheetTotal));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 3)]
    [InlineData(3, 3)]
    public void Build_AcceptsSheetNumbersInsideTheRange(int sheetNumber, int sheetTotal)
    {
        var (plan, accessories) = ReferencePlan();

        var model = DrawingSheetStampBuilder.Build(plan, accessories, sheetNumber, sheetTotal);

        Assert.Equal($"FOLHA {sheetNumber} DE {sheetTotal}", model.SheetLabel);
    }

    [Fact]
    public void Build_TrimsRiggerRegistrationNumbers()
    {
        var (plan, accessories) = ReferencePlan();
        plan.RiggerRpp = " 23-1040 ";
        plan.RiggerCrea = " 78755/D ";

        var model = DrawingSheetStampBuilder.Build(plan, accessories, 1, 1);

        Assert.Equal("RIGGER - RPP-23-1040", model.RiggerLabel);
        Assert.Equal("CREA-PR 78755/D", model.CreaLabel);
    }

    [Fact]
    public void Build_IsCultureInvariant()
    {
        var (plan, accessories) = ReferencePlan();

        var ptBr = RunUnderCulture("pt-BR", () => DrawingSheetStampBuilder.Build(plan, accessories, 1, 3));
        var enUs = RunUnderCulture("en-US", () => DrawingSheetStampBuilder.Build(plan, accessories, 1, 3));
        var deDe = RunUnderCulture("de-DE", () => DrawingSheetStampBuilder.Build(plan, accessories, 1, 3));

        Assert.Equal(ptBr, enUs);
        Assert.Equal(ptBr, deDe);
    }

    private static (RiggingPlan Plan, List<PlanAccessory> Accessories) ReferencePlan()
    {
        var plan = new RiggingPlan
        {
            PlanNumber = "2607036",
            PlanDate = new DateTime(2026, 7, 24),
            OperationName = "Chiller",
            ClientName = "Kaefer Rip Serviços Industriais Ltda.",
            LoadDescription = "Chiller",
            LengthM = 4.7m,
            WidthM = 1.95m,
            HeightM = 2.3m,
            WeightKg = 3450m,
            LoadCapacityT = 7.95m,
            FootingForceKgf = 7914m,
            MaxWindSpeedMs = 7.0m,
            RiggerName = "Giuliano Breda",
            RiggerRpp = "23-1040",
            RiggerCrea = "78755/D",
            ArtNumber = "1720264496217",
        };

        var accessories = new List<PlanAccessory>
        {
            new() { Section = "lingada", Description = "Cinta plana 3 m", Quantity = 4m, UnitWeightKg = 4.87m, SortOrder = 1 },
            new() { Section = "lingada", Description = "Manilha 7/8", Quantity = 6m, UnitWeightKg = 1.7m, SortOrder = 2 },
            new() { Section = "lingada", Description = "Cinta plana 4 m", Quantity = 2m, UnitWeightKg = 5.97m, SortOrder = 3 },
            new() { Section = "lingada", Description = "Manilha 1", Quantity = 2m, UnitWeightKg = 2.5m, SortOrder = 4 },
            new() { Section = "dispositivo", Description = "Balancim", Quantity = 1m, UnitWeightKg = 250m, SortOrder = 5 },
        };

        return (plan, accessories);
    }

    private static T RunUnderSystemCultures<T>(Func<T> action) => RunUnderCulture("pt-BR", action);

    private static T RunUnderCulture<T>(string cultureName, Func<T> action)
    {
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            var culture = new CultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }
}
