using System.Globalization;
using RiggingPlanner.Domain.Entities;
using Xunit;

namespace RiggingPlanner.Domain.Tests;

public class SummarySheetBuilderTests
{
    [Fact]
    public void Build_SortsAccessoriesAndNumbersSequentially()
    {
        var (plan, accessories) = CreatePlan();
        accessories.Add(Item("lingada", 2, "Cinta B", 2m, 3m));
        accessories.Add(Item("dispositivo", 1, "Balancim", 1m, 250m, drawingRef: "D-1"));
        accessories.Add(Item("lingada", 1, "Cinta A", 1m, 4m));

        var model = RunUnderSystemCultures(() => SummarySheetBuilder.Build(plan, accessories));

        Assert.True(model.SlingLines.Count == 2);
        Assert.True(model.DeviceLines.Count == 1);
        Assert.Equal("1", model.SlingLines[0].Number.ToString());
        Assert.Equal("Cinta A", model.SlingLines[0].Description);
        Assert.Equal("2", model.SlingLines[1].Number.ToString());
        Assert.Equal("Cinta B", model.SlingLines[1].Description);
        Assert.Equal("3", model.DeviceLines[0].Number.ToString());
        Assert.Equal("Balancim", model.DeviceLines[0].Description);
    }

    [Fact]
    public void Build_SlingLinesHideDrawingRefAndDeviceLinesShowIt()
    {
        var (plan, accessories) = CreatePlan();
        accessories.Add(Item("lingada", 1, "Cinta", 1m, 4m, drawingRef: "D-1"));
        accessories.Add(Item("dispositivo", 2, "Balancim", 1m, 250m, drawingRef: "D-2"));

        var model = RunUnderSystemCultures(() => SummarySheetBuilder.Build(plan, accessories));

        Assert.Equal(string.Empty, model.SlingLines[0].DrawingRef);
        Assert.Equal("D-2", model.DeviceLines[0].DrawingRef);
    }

    [Fact]
    public void Build_ComputesSectionTotalsAndGrossWeight()
    {
        var (plan, accessories) = CreatePlan();
        plan.WeightKg = 3450m;
        plan.ExtraLoadKg = 100m;
        accessories.Add(Item("lingada", 1, "Cinta", 4m, 4.87m));
        accessories.Add(Item("dispositivo", 2, "Balancim", 1m, 250m));

        var model = RunUnderSystemCultures(() => SummarySheetBuilder.Build(plan, accessories));

        Assert.Equal("19,48", model.SlingTotal);
        Assert.Equal("250", model.DeviceTotal);
        Assert.Equal("19,48", model.SlingWeightKg);
        Assert.Equal("250", model.DeviceWeightKg);
        Assert.Equal("100", model.ExtraLoadKg);
        Assert.Equal("3819", model.GrossWeightKg);
        Assert.Equal("3,82", model.GrossWeightTons);
    }

    [Fact]
    public void Build_UtilizationUsesGrossWeightTonsAndLoadCapacity()
    {
        var (plan, accessories) = CreatePlan();
        plan.WeightKg = 3450m;
        plan.LoadCapacityT = 7.95m;
        accessories.Add(Item("lingada", 1, "Cinta", 4m, 4.87m));
        accessories.Add(Item("dispositivo", 2, "Balancim", 1m, 250m));

        var model = RunUnderSystemCultures(() => SummarySheetBuilder.Build(plan, accessories));

        // 3,71948 t / 7,95 t * 100 = 46,786... -> one decimal, away from zero.
        Assert.Equal("46,8", model.CraneUtilizationPct);
        Assert.Equal("7,95", model.CraneLoadCapacityT);
    }

    [Theory]
    [InlineData(null, "MD 45.007", "MD 45.007")]
    [InlineData("Palfinger", "MD 45.007", "Palfinger MD 45.007")]
    [InlineData("Palfinger", null, "Palfinger")]
    [InlineData(null, null, "-")]
    [InlineData("Palfinger", "Palfinger MD", "Palfinger MD")]
    public void Build_ModelJoinsBrandAndModelWithoutRepetition(string? brand, string? model, string expected)
    {
        var (plan, accessories) = CreatePlan();
        plan.CraneBrand = brand;
        plan.CraneModel = model;

        var sheet = RunUnderSystemCultures(() => SummarySheetBuilder.Build(plan, accessories));

        Assert.Equal(expected, sheet.CraneModel);
    }

    [Theory]
    [InlineData("Giuliano Breda", "23-1040", "Rigger Giuliano Breda - RPP-23-1040")]
    [InlineData("Giuliano Breda", null, "Rigger Giuliano Breda")]
    [InlineData(null, "23-1040", "Rigger - - RPP-23-1040")]
    [InlineData(null, null, "Rigger -")]
    public void Build_SignaturePrintsRiggerNameAndRpp(string? name, string? rpp, string expected)
    {
        var (plan, accessories) = CreatePlan();
        plan.RiggerName = name;
        plan.RiggerRpp = rpp;

        var sheet = RunUnderSystemCultures(() => SummarySheetBuilder.Build(plan, accessories));

        Assert.Equal(expected, sheet.RiggerSignature);
    }

    private static (RiggingPlan Plan, List<PlanAccessory> Accessories) CreatePlan() =>
        (new RiggingPlan { PlanNumber = "2611001", PlanDate = new DateTime(2026, 11, 17) }, []);

    private static PlanAccessory Item(string section, int sortOrder, string description, decimal quantity, decimal unitWeightKg, string? drawingRef = null) =>
        new()
        {
            Section = section,
            Description = description,
            DrawingRef = drawingRef,
            Quantity = quantity,
            UnitWeightKg = unitWeightKg,
            SortOrder = sortOrder,
        };

    private static T RunUnderSystemCultures<T>(Func<T> action)
    {
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            var culture = new CultureInfo("pt-BR");
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
