using System.Globalization;
using Xunit;

namespace RiggingPlanner.Domain.Tests;

public class PlanValueFormatterTests
{
    private const string NullMarker = "-";

    [Theory]
    [InlineData(null, NullMarker)]
    [InlineData("4.7", "4,7")]
    [InlineData("1.95", "1,95")]
    [InlineData("2.3", "2,3")]
    [InlineData("3450", "3450")]
    [InlineData("4.87", "4,87")]
    [InlineData("19.48", "19,48")]
    [InlineData("1.7", "1,7")]
    [InlineData("5", "5")]
    [InlineData("46.62", "46,62")]
    [InlineData("250", "250")]
    [InlineData("3.17", "3,17")]
    [InlineData("7914", "7914")]
    [InlineData("0", "0")]
    [InlineData("0.005", "0,01")]
    [InlineData("2.345", "2,35")]
    [InlineData("10.0", "10")]
    [InlineData("-4.7", "-4,7")]
    [InlineData("-2.345", "-2,35")]
    [InlineData("-0.005", "-0,01")]
    [InlineData("0.004", "0")]
    public void Trim2_FormatsExpected(string? input, string expected) =>
        RunUnderSystemCultures(() => Assert.Equal(expected, PlanValueFormatter.Trim2(Parse(input))));

    [Theory]
    [InlineData(null, NullMarker)]
    [InlineData("45", "45,0")]
    [InlineData("45.0", "45,0")]
    [InlineData("5.1", "5,1")]
    [InlineData("5.0", "5,0")]
    [InlineData("7.95", "7,95")]
    [InlineData("60.0", "60,0")]
    [InlineData("70.0", "70,0")]
    [InlineData("7", "7,0")]
    [InlineData("10.0", "10,0")]
    [InlineData("10.123", "10,12")]
    [InlineData("10.125", "10,13")]
    [InlineData("-7", "-7,0")]
    public void Min1Max2_FormatsExpected(string? input, string expected) =>
        RunUnderSystemCultures(() => Assert.Equal(expected, PlanValueFormatter.Min1Max2(Parse(input))));

    [Theory]
    [InlineData(null, NullMarker)]
    [InlineData("47.1", "47,1")]
    [InlineData("47.14", "47,1")]
    [InlineData("47.15", "47,2")]
    [InlineData("47", "47,0")]
    [InlineData("-47.15", "-47,2")]
    public void Fixed1_FormatsExpected(string? input, string expected) =>
        RunUnderSystemCultures(() => Assert.Equal(expected, PlanValueFormatter.Fixed1(Parse(input))));

    [Theory]
    [InlineData(null, NullMarker)]
    [InlineData("3.75", "3,75")]
    [InlineData("3.7", "3,70")]
    [InlineData("3", "3,00")]
    [InlineData("-3.75", "-3,75")]
    public void Fixed2_FormatsExpected(string? input, string expected) =>
        RunUnderSystemCultures(() => Assert.Equal(expected, PlanValueFormatter.Fixed2(Parse(input))));

    [Theory]
    [InlineData(null, NullMarker)]
    [InlineData("3747", "3747")]
    [InlineData("3746.62", "3747")]
    [InlineData("7914", "7914")]
    [InlineData("3450", "3450")]
    [InlineData("0", "0")]
    [InlineData("-3746.62", "-3747")]
    public void Int0_FormatsExpected(string? input, string expected) =>
        RunUnderSystemCultures(() => Assert.Equal(expected, PlanValueFormatter.Int0(Parse(input))));

    [Fact]
    public void ReferencePlan_GrossWeight_MatchesExample()
    {
        // Lingada 46,62 + dispositivos 250 + carga liquida 3450 + extras 0 = 3746,62 kg.
        const decimal lingada = 46.62m;
        const decimal dispositivos = 250m;
        const decimal liquid = 3450m;
        const decimal extras = 0m;
        var grossWeightKg = liquid + lingada + dispositivos + extras;

        // Same formula as CapacityState: no rounding before dividing.
        var grossWeightTons = grossWeightKg / 1000m;
        const decimal loadCapacityT = 7.95m;
        var utilization = grossWeightTons / loadCapacityT * 100m;

        RunUnderSystemCultures(() =>
        {
            Assert.Equal("3747", PlanValueFormatter.Int0(grossWeightKg));
            Assert.Equal("3,75", PlanValueFormatter.Fixed2(grossWeightTons));
            Assert.Equal("47,1", PlanValueFormatter.Fixed1(utilization));
            Assert.Equal("46,62", PlanValueFormatter.Trim2(lingada));
            Assert.Equal("250", PlanValueFormatter.Trim2(dispositivos));
            Assert.Equal("3450", PlanValueFormatter.Trim2(liquid));
            Assert.Equal("7,95", PlanValueFormatter.Min1Max2(loadCapacityT));
        });
    }

    [Theory]
    [InlineData(null, NullMarker)]
    [InlineData("2026-07-24", "24/07/2026")]
    [InlineData("2026-11-16", "16/11/2026")]
    public void Date_FormatsExpected(string? input, string expected)
    {
        var date = input is null ? null : (DateTime?)DateTime.Parse(input, CultureInfo.InvariantCulture);
        RunUnderSystemCultures(() => Assert.Equal(expected, PlanValueFormatter.Date(date)));
    }

    [Theory]
    [InlineData(null, NullMarker)]
    [InlineData("", NullMarker)]
    [InlineData("   ", NullMarker)]
    [InlineData("\t", NullMarker)]
    [InlineData("Içamento Chiller", "Içamento Chiller")]
    public void Text_FormatsExpected(string? input, string expected) =>
        RunUnderSystemCultures(() => Assert.Equal(expected, PlanValueFormatter.Text(input)));

    [Fact]
    public void Formatters_NeverUseThousandSeparators()
    {
        RunUnderSystemCultures(() =>
        {
            Assert.DoesNotContain(".", PlanValueFormatter.Trim2(12345.67m));
            Assert.DoesNotContain(".", PlanValueFormatter.Int0(1234567m));
            Assert.DoesNotContain(".", PlanValueFormatter.Fixed2(1234567.123m));
        });
    }

    private static decimal? Parse(string? input) =>
        input is null ? null : decimal.Parse(input, CultureInfo.InvariantCulture);

    private static void RunUnderSystemCultures(Action assertion)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        var originalDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        try
        {
            foreach (var culture in new[] { new CultureInfo("en-US"), new CultureInfo("de-DE") })
            {
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                assertion();
            }
        }
        finally
        {
            CultureInfo.DefaultThreadCurrentCulture = originalDefaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = originalDefaultUiCulture;
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }
}
