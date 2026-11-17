using System.Globalization;
using System.Text.RegularExpressions;
using RiggingPlanner.Domain;
using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Infrastructure.Pdf;
using RiggingPlanner.Infrastructure.Tests.Fixtures;
using RiggingPlanner.Infrastructure.Tests.Pdf.Tools;
using UglyToad.PdfPig;
using Xunit.Abstractions;

namespace RiggingPlanner.Infrastructure.Tests.Pdf;

/// <summary>
/// Stage-1 checks of the summary sheet (page 1) against the example document
/// (Fixtures/reference-plan.pdf): single A4 page with Carlito, letter-level
/// position agreement, all texts present, overflow scaling and dash printing.
/// </summary>
public sealed class SummarySheetPdfTests(ITestOutputHelper output)
{
    private const double TolerancePt = 1.0;
    private const int Stage = 1;

    [Fact]
    public void Sheet_is_one_A4_page_with_Carlito()
    {
        var pdf = RenderReferenceSheet();

        using var document = PdfDocument.Open(pdf);
        Assert.Equal(1, document.NumberOfPages);

        var page = document.GetPages().Single();
        Assert.InRange((double)page.Width, SummarySheetLayout.PageWidth - 0.5, SummarySheetLayout.PageWidth + 0.5);
        Assert.InRange((double)page.Height, SummarySheetLayout.PageHeight - 0.5, SummarySheetLayout.PageHeight + 0.5);

        var wrongFont = page.Letters
            .Where(l => !(l.FontName ?? string.Empty).Contains("Carlito", StringComparison.OrdinalIgnoreCase))
            .Select(l => $"{l.Value} font {l.FontName}")
            .ToList();
        Assert.True(wrongFont.Count == 0, "Letters not using Carlito: " + string.Join(", ", wrongFont.Take(10)));
    }

    [Fact]
    public void Letters_match_the_example_within_tolerance()
    {
        var pdf = RenderReferenceSheet();
        var examplePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "reference-plan.pdf");
        var exampleBytes = File.ReadAllBytes(examplePath);

        var example = PdfTextReader.ReadLetterPages(exampleBytes)[0];
        var generated = PdfTextReader.ReadLetterPages(pdf)[0];

        // Artifacts for human review (stage-1 doc, section 6).
        ReviewOutput.WriteFile(Stage, "summary-sheet.pdf", pdf);
        var generatedPng = PdfRasterizer.RasterizeToPng(pdf);
        var examplePng = PdfRasterizer.RasterizeToPng(exampleBytes);
        ReviewOutput.WriteFile(Stage, "summary-sheet.png", generatedPng);
        var overlay = PdfOverlay.Compose(examplePng, generatedPng);
        ReviewOutput.WriteFile(Stage, "overlay.png", overlay.Png);

        // Exception windows (points, top-left origin), valid for both documents at the
        // reference plan (no overflow):
        // 1) device totals label row: the generated label has its own text (intentional
        //    difference #1), right-aligned at the same right edge; the row's value
        //    (x > 490) keeps being compared;
        // 2) "Capacidade de carga" configuration value: the plan stores 7,95 while the
        //    example shows 7,9 (intentional difference #2).
        static bool IsExcepted(double baseline, double x) =>
            (baseline is >= 330.9 and <= 332.6 && x is >= 230 and <= 490)
            || (baseline is >= 496.9 and <= 498.5 && x is >= 220 and <= 260);

        // Adjustment A: each EXAMPLE letter must find the same letter in the GENERATED
        // sheet. Whitespace glyphs are ignored (the example emits space glyphs only in
        // some runs; positions of real letters already pin every run).
        var generatedPool = generated.Letters
            .Where(l => l.Text.Trim().Length > 0 && !IsExcepted(l.Baseline, l.X))
            .Select(l => new LetterCandidate(l.Text, l.X, l.Baseline, l.Size))
            .ToList();

        var diffs = new List<string>();
        foreach (var letter in example.Letters)
        {
            if (letter.Text.Trim().Length == 0 || IsExcepted(letter.Baseline, letter.X))
            {
                continue;
            }

            var best = -1;
            var bestDistance = double.MaxValue;
            for (var i = 0; i < generatedPool.Count; i++)
            {
                var candidate = generatedPool[i];
                if (candidate.Used
                    || candidate.Text != letter.Text
                    || Math.Abs(candidate.Size - letter.Size) > 0.05
                    || Math.Abs(candidate.Baseline - letter.Baseline) > 2
                    || Math.Abs(candidate.X - letter.X) > 25)
                {
                    continue;
                }

                var distance = Math.Abs(candidate.X - letter.X) + Math.Abs(candidate.Baseline - letter.Baseline);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            if (best < 0)
            {
                diffs.Add($"UNMATCHED_EX\t{letter.Text}\tx={letter.X:F2}\tbaseline={letter.Baseline:F2}\tsize={letter.Size:F2}");
                continue;
            }

            generatedPool[best].Used = true;
            var deltaX = generatedPool[best].X - letter.X;
            var deltaBaseline = generatedPool[best].Baseline - letter.Baseline;
            if (Math.Abs(deltaX) > TolerancePt || Math.Abs(deltaBaseline) > TolerancePt)
            {
                diffs.Add($"OFF\t{letter.Text}\tdx={deltaX:F2}\tdbaseline={deltaBaseline:F2}\tex=({letter.X:F2},{letter.Baseline:F2})\tgen=({generatedPool[best].X:F2},{generatedPool[best].Baseline:F2})");
            }
        }

        foreach (var extra in generatedPool.Where(c => !c.Used))
        {
            diffs.Add($"UNMATCHED_GEN\t{extra.Text}\tx={extra.X:F2}\tbaseline={extra.Baseline:F2}\tsize={extra.Size:F2}");
        }

        var sorted = diffs
            .Select(d => (Line: d, Magnitude: Math.Abs(ParseDelta(d, "dx")) + Math.Abs(ParseDelta(d, "dbaseline"))))
            .OrderByDescending(d => d.Magnitude)
            .Select(d => d.Line)
            .ToList();

        var report = new List<string> { $"letter diffs: {diffs.Count} (tolerance {TolerancePt}pt)" };
        report.AddRange(sorted.Take(10));
        var reportPath = ReviewOutput.WriteText(Stage, "letter-diff-report.txt", string.Join(Environment.NewLine, report));

        var metrics = new List<string>
        {
            $"example letters: {example.Letters.Count}, generated letters: {generated.Letters.Count}",
            $"example size: {example.Width:F2} x {example.Height:F2} pt, generated size: {generated.Width:F2} x {generated.Height:F2} pt",
            $"letter diffs: {diffs.Count} (tolerance {TolerancePt} pt on X and baseline)",
            $"overlay: {overlay.DivergentPixels} divergent pixels of {overlay.TotalPixels} ({overlay.DivergencePercent:F3}%)",
        };
        ReviewOutput.WriteText(Stage, "diff-metrics.txt", string.Join(Environment.NewLine, metrics));

        output.WriteLine($"report: {reportPath}");
        output.WriteLine(string.Join(Environment.NewLine, report));

        Assert.True(diffs.Count == 0, $"{diffs.Count} letter diffs (tolerance {TolerancePt}pt). See {reportPath}");
    }

    [Fact]
    public void Every_example_word_appears_in_the_generated_sheet()
    {
        var pdf = RenderReferenceSheet();
        var examplePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "reference-plan.pdf");

        var exampleWords = PdfTextReader.ReadPages(File.ReadAllBytes(examplePath))[0].Words;
        var generatedText = string.Join(" ", PdfTextReader.ReadPages(pdf)[0].Words.Select(w => w.Text));
        var normalizedGenerated = Normalize(generatedText);

        var missing = new List<string>();
        foreach (var word in exampleWords)
        {
            var normalized = Normalize(word.Text);

            // "7,9" is the example's rounded load capacity; the plan stores 7,95
            // (intentional difference #2), which contains "7,9" anyway.
            if (normalized.Length == 0 || normalized == "7,9")
            {
                continue;
            }

            if (!normalizedGenerated.Contains(normalized, StringComparison.Ordinal))
            {
                missing.Add(word.Text);
            }
        }

        Assert.True(missing.Count == 0, "Words missing in the generated sheet: " + string.Join(", ", missing.Take(15)));
    }

    [Fact]
    public void Excess_items_scale_down_to_a_single_A4_page()
    {
        var (plan, _) = TestPlans.ReferencePlan();
        var accessories = new List<PlanAccessory>();
        for (var i = 1; i <= 12; i++)
        {
            accessories.Add(new PlanAccessory
            {
                Section = PlanTotals.SlingSection,
                Description = LongDescription(i),
                Quantity = 2m,
                UnitWeightKg = 12.5m,
                SortOrder = i,
            });
        }

        for (var i = 1; i <= 4; i++)
        {
            accessories.Add(new PlanAccessory
            {
                Section = PlanTotals.DeviceSection,
                Description = LongDescription(i),
                DrawingRef = "-",
                Quantity = 1m,
                UnitWeightKg = 250m,
                SortOrder = i,
            });
        }

        var model = SummarySheetBuilder.Build(plan, accessories);
        Assert.Equal(120, LongDescription(1).Length);

        var pdf = new SummarySheetRenderer().Render(model);
        ReviewOutput.WriteFile(Stage, "excess-summary-sheet.pdf", pdf);
        ReviewOutput.WriteFile(Stage, "excess-summary-sheet.png", PdfRasterizer.RasterizeToPng(pdf));

        using var document = PdfDocument.Open(pdf);
        Assert.Equal(1, document.NumberOfPages);
        var page = document.GetPages().Single();
        Assert.InRange((double)page.Width, SummarySheetLayout.PageWidth - 0.5, SummarySheetLayout.PageWidth + 0.5);
        Assert.InRange((double)page.Height, SummarySheetLayout.PageHeight - 0.5, SummarySheetLayout.PageHeight + 0.5);

        // Nothing may be lost: every generated description must appear on the page.
        var pageText = string.Join(" ", PdfTextReader.ReadPages(pdf)[0].Words.Select(w => w.Text));
        var normalizedPageText = Normalize(pageText);
        foreach (var description in model.SlingLines.Select(l => l.Description)
                     .Concat(model.DeviceLines.Select(l => l.Description)))
        {
            Assert.True(
                normalizedPageText.Contains(Normalize(description), StringComparison.Ordinal),
                $"Missing description: {description}");
        }

        // The content must be reduced: the 12th sling row sits above its unscaled baseline.
        var extraSlingRows = model.SlingLines.Count - 4;
        var extraDeviceRows = model.DeviceLines.Count - 1;
        var deviceShift = (extraSlingRows + extraDeviceRows) * SummarySheetLayout.RowStep;
        var scale = SummarySheetLayout.PageHeight / (SummarySheetLayout.PageHeight + deviceShift);
        var contentCenterY = (SummarySheetLayout.PageHeight + deviceShift) / 2;
        var expectedBaseline = SummarySheetLayout.PageCenterY
            + (SummarySheetLayout.SlingRowNumberBaseline + 11 * SummarySheetLayout.RowStep - contentCenterY) * scale;

        var letters = PdfTextReader.ReadLetterPages(pdf)[0].Letters;
        Assert.True(
            letters.Any(l => Math.Abs(l.Baseline - expectedBaseline) < 0.5),
            $"12th sling row not found near baseline {expectedBaseline:F2} (scale {scale:F3})");
    }

    [Fact]
    public void Optional_fields_print_dash_when_null()
    {
        var (plan, accessories) = TestPlans.ReferencePlan();
        plan.ClientName = null;
        plan.LengthM = null;
        plan.WidthM = null;
        plan.HeightM = null;
        plan.WeightKg = null;
        plan.ExtraLoadKg = null;
        plan.TruckModel = null;
        plan.LicensePlate = null;
        plan.ChassisNumber = null;
        plan.ManufactureYear = null;
        plan.CraneBrand = null;
        plan.CraneModel = null;
        plan.CraneNominalCapacity = null;
        plan.BoomLengthM = null;
        plan.OperatingRadiusM = null;
        plan.LoadCapacityT = null;
        plan.QuadrantStart = null;
        plan.QuadrantEnd = null;
        plan.FlatStrapUtilizationPct = null;
        plan.TubularStrapUtilizationPct = null;
        plan.WireRopeSlingUtilizationPct = null;
        plan.ShackleUtilizationPct = null;
        plan.FootingForceKgf = null;
        plan.MinGroundResistanceKgfCm2 = null;
        plan.MaxWindSpeedMs = null;
        plan.RiggerName = null;
        plan.RiggerRpp = null;
        plan.ArtNumber = null;
        plan.Observations = null;

        var model = SummarySheetBuilder.Build(plan, accessories);

        Assert.Equal("-", model.ClientName);
        Assert.Equal("-", model.LengthM);
        Assert.Equal("-", model.WidthM);
        Assert.Equal("-", model.HeightM);
        Assert.Equal("-", model.WeightKg);
        Assert.Equal("-", model.ExtraLoadKg);
        Assert.Equal("-", model.TruckModel);
        Assert.Equal("-", model.LicensePlate);
        Assert.Equal("-", model.ChassisNumber);
        Assert.Equal("-", model.ManufactureYear);
        Assert.Equal("-", model.CraneModel);
        Assert.Equal("-", model.CraneNominalCapacity);
        Assert.Equal("-", model.BoomLengthM);
        Assert.Equal("-", model.OperatingRadiusM);
        Assert.Equal("-", model.LoadCapacityT);
        Assert.Equal("-", model.CraneLoadCapacityT);
        Assert.Equal("-", model.CraneUtilizationPct);
        Assert.Equal("-", model.QuadrantStart);
        Assert.Equal("-", model.QuadrantEnd);
        Assert.Equal("-", model.FlatStrapUtilizationPct);
        Assert.Equal("-", model.TubularStrapUtilizationPct);
        Assert.Equal("-", model.WireRopeSlingUtilizationPct);
        Assert.Equal("-", model.ShackleUtilizationPct);
        Assert.Equal("-", model.FootingForceKgf);
        Assert.Equal("-", model.MinGroundResistanceKgfCm2);
        Assert.Equal("-", model.MaxWindSpeedMs);
        Assert.Equal("-", model.ArtDisplay);
        Assert.Equal("Rigger -", model.RiggerSignature);
        Assert.Equal(string.Empty, model.Observations);

        var pdf = new SummarySheetRenderer().Render(model);
        using var document = PdfDocument.Open(pdf);
        Assert.Equal(1, document.NumberOfPages);

        var dashCount = document.GetPages().Single().Letters.Count(l => l.Value == "-");
        Assert.True(dashCount >= 20, $"Expected at least 20 dashes for null optionals, found {dashCount}.");
    }

    private static byte[] RenderReferenceSheet()
    {
        var (plan, accessories) = TestPlans.ReferencePlan();
        var model = SummarySheetBuilder.Build(plan, accessories);
        return new SummarySheetRenderer().Render(model);
    }

    private static string LongDescription(int i)
    {
        var text = $"Acessório de içamento número {i} com descrição longa para validar a escala da folha resumo";
        while (text.Length < 120)
        {
            text += " e detalhes";
        }

        return text[..120];
    }

    private static string Normalize(string value) => Regex.Replace(value, @"\s+", string.Empty);

    private static double ParseDelta(string line, string label)
    {
        var marker = line.IndexOf(label + "=", StringComparison.Ordinal);
        if (marker < 0)
        {
            return 0;
        }

        var start = marker + label.Length + 1;
        var end = line.IndexOfAny(['\t', '\n'], start);
        if (end < 0)
        {
            end = line.Length;
        }

        return double.TryParse(line[start..end].TrimEnd('p', 't'), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private sealed class LetterCandidate(string Text, double X, double Baseline, double Size)
    {
        public string Text { get; } = Text;

        public double X { get; } = X;

        public double Baseline { get; } = Baseline;

        public double Size { get; } = Size;

        public bool Used;
    }
}
