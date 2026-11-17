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
/// Stage-2A checks of the drawing sheet (frame and stamp) against the example
/// (Fixtures/drawing-sheet.reference.json and page 2 of reference-plan-2607036.pdf):
/// measured lines, texts inside their cells, glyph-ink anchoring, region overlay,
/// long texts and dash printing.
/// </summary>
public sealed class DrawingSheetPdfTests(ITestOutputHelper output)
{
    private const string Stage = "stage-2a";
    private const double LineTolerancePt = 0.05;
    private const double AnchoringTolerancePt = 1.0;
    private const double CellMarginPt = 1.0;

    [Fact]
    public void Layout_lines_match_the_reference_json()
    {
        var reference = ReferenceSheet.Load(Path.Combine(AppContext.BaseDirectory, "Fixtures", "drawing-sheet.reference.json"));

        Assert.Equal(reference.Lines.Count, DrawingSheetLayout.Lines.Count);

        var diffs = new List<string>();
        for (var i = 0; i < reference.Lines.Count; i++)
        {
            var expected = reference.Lines[i];
            var actual = DrawingSheetLayout.Lines[i];
            Compare("x0", expected.X0, actual.X0);
            Compare("x1", expected.X1, actual.X1);
            Compare("top", expected.Top, actual.Top);
            Compare("bottom", expected.Bottom, actual.Bottom);
            Compare("lw", expected.Width, actual.Width);
        }

        void Compare(string field, double expected, double actual)
        {
            var delta = Math.Abs(expected - actual);
            if (delta > LineTolerancePt)
            {
                diffs.Add($"line {diffs.Count + 1} {field}: expected {expected:F2}, actual {actual:F2}, delta {delta:F2}");
            }
        }

        Assert.True(diffs.Count == 0, string.Join(Environment.NewLine, diffs));

        // The reference JSON only lists lines with width >= 0.99 pt; the 0.51 pt
        // signature line is checked against the values documented in section 2.
        Assert.Equal(78.8, DrawingSheetLayout.SignatureLine.X0, 2);
        Assert.Equal(188.4, DrawingSheetLayout.SignatureLine.X1, 2);
        Assert.Equal(729.29, DrawingSheetLayout.SignatureLine.Top, 2);
        Assert.Equal(0.51, DrawingSheetLayout.SignatureLine.Width, 2);
        Assert.True(reference.Lines.All(l => l.Width >= 0.99));
    }

    [Fact]
    public void Stamp_texts_are_present_and_inside_their_cells()
    {
        var (model, content, pdf) = RenderReference();
        var letters = PdfTextReader.ReadLetterPages(pdf)[0].Letters;
        var pageText = Normalize(string.Join(" ", PdfTextReader.ReadPages(pdf)[0].Words.Select(w => w.Text)));

        var expectedTexts = new[]
        {
            "DESCRIÇÃO DA CARGA", "COMPRIMENTO", "LARGURA", "ALTURA", "PESO", "[m]", "[kg]",
            "Chiller", "4,7", "1,95", "2,3", "3450",
            "CARGA BRUTA", "3,75", "t", "CAPACIDADE CARGA", "7,95", "TAXA DE UTILIZAÇÃO", "47,1", "%",
            "ESFORÇO NAS SAPATAS", "7914", "kgf", "VELOC. MÁX VENTO", "7,0", "m/s",
            "OPERAÇÃO:", "CLIENTE:", "Kaefer Rip Serviços Industriais Ltda.", "A4",
            "ART:", "1720264496217", "Nº Rigging:", "2607036",
            "Revisão:", "1 de 1", "ESCALA", "Diversas", "FOLHA 1 DE 3",
            "Giuliano Breda", "RIGGER - RPP-23-1040", "CREA-PR 78755/D",
            "Guindastes Ribas Ltda.", "CNPJ 81.076.242/0001-60", "Rua Justo Manfron, 2845 G",
            "Santa Felicidade - Curitiba - PR", "CEP 82.410-540",
            "4", "3", "2", "1", "F", "E", "D", "C", "B", "A",
        };

        var composed = content.Texts.Select(t => Normalize(t.Text)).ToHashSet(StringComparer.Ordinal);
        foreach (var expected in expectedTexts)
        {
            var normalized = Normalize(expected);
            Assert.True(composed.Contains(normalized), $"Layout is missing the text '{expected}'.");
            Assert.True(pageText.Contains(normalized, StringComparison.Ordinal), $"Generated PDF is missing the text '{expected}'.");
        }

        var problems = new List<string>();
        foreach (var run in content.Texts.Where(t => t.Cell is not null))
        {
            var found = FindGlyphs(letters, run);
            if (found is null)
            {
                problems.Add($"NOT FOUND: [{run.Text}] size {run.Size:F2}");
                continue;
            }

            var cell = run.Cell!.Value;
            var box = found.Box;
            var margin = Math.Min(
                Math.Min(box.Left - cell.Left, cell.Right - box.Right),
                Math.Min(box.Top - cell.Top, cell.Bottom - box.Bottom));
            if (margin < CellMarginPt)
            {
                problems.Add($"OUTSIDE: [{run.Text}] margin {margin:F2} pt, box ({box.Left:F2},{box.Top:F2},{box.Right:F2},{box.Bottom:F2}), cell ({cell.Left:F2},{cell.Top:F2},{cell.Right:F2},{cell.Bottom:F2})");
            }

            var wrongFont = found.Letters
                .Where(l => !l.FontName.Contains(ExpectedFont(run), StringComparison.OrdinalIgnoreCase))
                .Select(l => l.FontName)
                .Distinct()
                .ToList();
            if (wrongFont.Count > 0)
            {
                problems.Add($"FONT: [{run.Text}] expected {ExpectedFont(run)}, found {string.Join(", ", wrongFont)}");
            }
        }

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
        output.WriteLine($"{content.Texts.Count} text runs checked inside their cells.");
    }

    [Fact]
    public void Centered_texts_are_anchored_and_use_the_tabulated_sizes()
    {
        var (_, content, pdf) = RenderReference();
        var letters = PdfTextReader.ReadLetterPages(pdf)[0].Letters;
        var example = ReferenceSheet.Load(Path.Combine(AppContext.BaseDirectory, "Fixtures", "drawing-sheet.reference.json"));

        var deviations = new List<string>
        {
            "text\texample center\tgenerated center\tdelta x\tdelta y",
        };
        var problems = new List<string>();

        var runsPerCell = content.Texts
            .Where(t => t.Cell is not null)
            .GroupBy(t => t.Cell!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var run in content.Texts.Where(t => t.Align == DrawingSheetAlign.Center))
        {
            var found = FindGlyphs(letters, run);
            if (found is null)
            {
                problems.Add($"NOT FOUND: [{run.Text}]");
                continue;
            }

            var box = found.Box;
            var deltaX = box.CenterX - run.AnchorX;
            var deltaY = box.CenterY - run.AnchorY;
            if (Math.Abs(deltaX) > AnchoringTolerancePt || Math.Abs(deltaY) > AnchoringTolerancePt)
            {
                problems.Add($"ANCHOR: [{run.Text}] dx {deltaX:F2}, dy {deltaY:F2} (anchor {run.AnchorX:F2},{run.AnchorY:F2})");
            }

            var wrongSize = found.Letters.Where(l => Math.Abs(l.Size - run.Size) > 0.05).ToList();
            if (wrongSize.Count > 0)
            {
                problems.Add($"SIZE: [{run.Text}] expected {run.Size:F2}, found {wrongSize[0].Size:F2}");
            }

            // Cells with a single centered run must be centered on the cell itself.
            if (run.Cell is { } cell && runsPerCell[cell].Count == 1)
            {
                if (Math.Abs(run.AnchorX - cell.CenterX) > AnchoringTolerancePt
                    || Math.Abs(run.AnchorY - cell.CenterY) > AnchoringTolerancePt)
                {
                    problems.Add($"CELL: [{run.Text}] anchor {run.AnchorX:F2},{run.AnchorY:F2} vs cell center {cell.CenterX:F2},{cell.CenterY:F2}");
                }
            }

            var exampleBox = FindExampleBox(example.Words, run);
            deviations.Add(exampleBox is null
                ? $"{run.Text}\t-\t{box.CenterX:F2},{box.CenterY:F2}\t-\t-"
                : $"{run.Text}\t{exampleBox.Value.CenterX:F2},{exampleBox.Value.CenterY:F2}\t{box.CenterX:F2},{box.CenterY:F2}\t{box.CenterX - exampleBox.Value.CenterX:F2}\t{box.CenterY - exampleBox.Value.CenterY:F2}");
        }

        var sizes = new (string Text, double Size)[]
        {
            ("DESCRIÇÃO DA CARGA", DrawingSheetLayout.HeaderSize),
            ("COMPRIMENTO", DrawingSheetLayout.HeaderSize),
            ("[kg]", DrawingSheetLayout.HeaderSize),
            ("CARGA BRUTA", DrawingSheetLayout.LabelSize),
            ("3,75", DrawingSheetLayout.LabelSize),
            ("OPERAÇÃO:", DrawingSheetLayout.CornerLabelSize),
            ("CLIENTE:", DrawingSheetLayout.CornerLabelSize),
            ("A4", DrawingSheetLayout.PaperSize),
            ("ART:", DrawingSheetLayout.CornerLabelSize),
            ("1720264496217", DrawingSheetLayout.ArtNumberSize),
            ("2607036", DrawingSheetLayout.ArtNumberSize),
            ("Revisão:", DrawingSheetLayout.CornerLabelSize),
            ("1 de 1", DrawingSheetLayout.RevisionValueSize),
            ("ESCALA", DrawingSheetLayout.CornerLabelSize),
            ("FOLHA 1 DE 3", DrawingSheetLayout.CornerLabelSize),
            ("Giuliano Breda", DrawingSheetLayout.RiggerNameSize),
            ("RIGGER - RPP-23-1040", DrawingSheetLayout.RiggerLabelSize),
            ("CREA-PR 78755/D", DrawingSheetLayout.RiggerLabelSize),
            ("Guindastes Ribas Ltda.", DrawingSheetLayout.CompanyNameSize),
            ("CNPJ 81.076.242/0001-60", DrawingSheetLayout.CompanyLineSize),
            ("4", DrawingSheetLayout.ZoneLabelSize),
        };

        foreach (var (text, size) in sizes)
        {
            var run = content.Texts.First(t => t.Text == text);
            Assert.Equal(size, run.Size, 2);
        }

        // Operation and client may shrink to fit (minimum 8 pt), never above the table size.
        foreach (var label in new[] { DrawingSheetLayout.OperationLabel, DrawingSheetLayout.ClientLabel })
        {
            var cell = content.Texts.Single(t => t.Text == label).Cell!.Value;
            foreach (var value in content.Texts.Where(t => t.Cell == cell && t.Align == DrawingSheetAlign.Center))
            {
                Assert.InRange(value.Size, 8.0, DrawingSheetLayout.ClientValueSize);
            }
        }

        var reportPath = ReviewOutput.WriteText(Stage, "anchoring-deviations.txt", string.Join(Environment.NewLine, deviations));
        output.WriteLine($"deviation table: {reportPath}");
        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void Overlay_of_the_frame_and_stamp_region_is_reported()
    {
        var (_, _, pdf) = RenderReference();
        var examplePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "reference-plan-2607036.pdf");
        var exampleBytes = File.ReadAllBytes(examplePath);

        using (var example = PdfDocument.Open(exampleBytes))
        {
            Assert.True(example.NumberOfPages >= 2);
        }

        var examplePng = PdfRasterizer.RasterizeToPng(exampleBytes, page: 1);
        var generatedPng = PdfRasterizer.RasterizeToPng(pdf);

        // Frame and stamp region: everything outside the drawing views area
        // (x 57 -> 566, y 29 -> 661, points, top-left origin).
        static bool IncludePoint(double x, double y) => !(x >= 57 && x <= 566 && y >= 29 && y <= 661);

        var overlay = PdfOverlay.Compose(
            examplePng,
            generatedPng,
            IncludePoint,
            DrawingSheetLayout.PageWidth,
            DrawingSheetLayout.PageHeight);

        ReviewOutput.WriteFile(Stage, "drawing-sheet.pdf", pdf);
        ReviewOutput.WriteFile(Stage, "drawing-sheet.png", generatedPng);
        ReviewOutput.WriteFile(Stage, "overlay.png", overlay.Png);
        var metrics = new List<string>
        {
            $"overlay region: frame and stamp (views area x 57-566, y 29-661 excluded)",
            $"divergent pixels: {overlay.DivergentPixels} of {overlay.TotalPixels} ({overlay.DivergencePercent:F3}%)",
        };
        var metricsPath = ReviewOutput.WriteText(Stage, "diff-metrics.txt", string.Join(Environment.NewLine, metrics));
        output.WriteLine(string.Join(Environment.NewLine, metrics));
        output.WriteLine($"artifacts: {metricsPath}");

        Assert.True(overlay.TotalPixels > 0);
        Assert.InRange(overlay.DivergencePercent, 0, 40);
    }

    [Fact]
    public void Long_client_and_operation_fit_without_truncation()
    {
        var (plan, accessories) = TestPlans.Reference2607036();
        plan.ClientName = LongText("Cliente com nome muito longo para o carimbo", 60);
        plan.OperationName = LongText("Operação de içamento com descrição longa para o carimbo", 80);
        Assert.Equal(60, plan.ClientName.Length);
        Assert.Equal(80, plan.OperationName.Length);

        var model = DrawingSheetStampBuilder.Build(plan, accessories, 1, 3);
        var content = DrawingSheetLayout.Compose(model);
        var pdf = new DrawingSheetsRenderer().RenderFrameOnly(model);
        ReviewOutput.WriteFile(Stage, "long-texts.pdf", pdf);
        ReviewOutput.WriteFile(Stage, "long-texts.png", PdfRasterizer.RasterizeToPng(pdf));

        var letters = PdfTextReader.ReadLetterPages(pdf)[0].Letters;
        var words = PdfTextReader.ReadPages(pdf)[0].Words;
        var pageText = Normalize(string.Join(" ", words.Select(w => w.Text)));
        ReviewOutput.WriteText(
            Stage,
            "long-texts-words.txt",
            string.Join(Environment.NewLine, words.Select(w => $"[{w.Text}] x0={w.X0:F2} x1={w.X1:F2} top={w.Top:F2} bottom={w.Bottom:F2}")));

        var problems = new List<string>();
        foreach (var run in content.Texts.Where(t => t.Cell is not null))
        {
            var found = FindGlyphs(letters, run);
            if (found is null)
            {
                problems.Add($"NOT FOUND: [{run.Text}]");
                continue;
            }

            var cell = run.Cell!.Value;
            var box = found.Box;
            var margin = Math.Min(
                Math.Min(box.Left - cell.Left, cell.Right - box.Right),
                Math.Min(box.Top - cell.Top, cell.Bottom - box.Bottom));
            if (margin < CellMarginPt)
            {
                problems.Add($"OUTSIDE: [{run.Text}] margin {margin:F2} pt");
            }

            // A truncated run would lose glyphs on one side and move its ink center.
            var expected = ExpectedInkBox(run);
            if (Math.Abs(box.CenterX - expected.CenterX) > 1.5 || Math.Abs(box.CenterY - expected.CenterY) > 1.5)
            {
                problems.Add($"TRUNCATED: [{run.Text}] ink center {box.CenterX:F2},{box.CenterY:F2} vs expected {expected.CenterX:F2},{expected.CenterY:F2}");
            }
        }

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));

        AssertValue(DrawingSheetLayout.ClientLabel, plan.ClientName);
        AssertValue(DrawingSheetLayout.OperationLabel, plan.OperationName);

        void AssertValue(string label, string expected)
        {
            var cell = content.Texts.Single(t => t.Text == label).Cell!.Value;
            var lines = content.Texts.Where(t => t.Cell == cell && t.Align == DrawingSheetAlign.Center).ToList();
            Assert.True(lines.Count > 0, $"No value runs for {label}.");
            Assert.All(lines, l => Assert.Equal(8.0, l.Size, 2));
            Assert.Equal(Normalize(expected), Normalize(string.Concat(lines.Select(l => l.Text))));

            // PdfPig interleaves runs when extracting, so presence is checked per word
            // (words of 3+ letters, which cannot match by accident).
            foreach (var word in expected.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var normalizedWord = Normalize(word);
                if (normalizedWord.Length >= 3)
                {
                    Assert.True(pageText.Contains(normalizedWord, StringComparison.Ordinal), $"PDF lost '{word}' of the {label} value.");
                }
            }
        }
    }

    [Fact]
    public void Null_optionals_print_dash()
    {
        var (plan, _) = TestPlans.Reference2607036();
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

        var model = DrawingSheetStampBuilder.Build(plan, [], 1, 1);

        Assert.Equal("-", model.LoadDescription);
        Assert.Equal("-", model.LengthM);
        Assert.Equal("-", model.WidthM);
        Assert.Equal("-", model.HeightM);
        Assert.Equal("-", model.WeightKg);
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

        var pdf = new DrawingSheetsRenderer().RenderFrameOnly(model);
        ReviewOutput.WriteFile(Stage, "null-optionals.pdf", pdf);
        using var document = PdfDocument.Open(pdf);
        var dashCount = document.GetPages().Single().Letters.Count(l => l.Value == "-");
        Assert.True(dashCount >= 15, $"Expected at least 15 dashes for null optionals, found {dashCount}.");
    }

    private static (DrawingSheetStampModel Model, DrawingSheetContent Content, byte[] Pdf) RenderReference()
    {
        var (plan, accessories) = TestPlans.Reference2607036();
        var model = DrawingSheetStampBuilder.Build(plan, accessories, 1, 3);
        var content = DrawingSheetLayout.Compose(model);
        return (model, content, new DrawingSheetsRenderer().RenderFrameOnly(model));
    }

    private static string ExpectedFont(DrawingSheetPlacedText run) =>
        run.Family == PdfFontRegistry.BrandFamilyName ? "OpenSans-SemiBold" : "Questrial";

    private static Box ExpectedInkBox(DrawingSheetPlacedText run)
    {
        var ink = DrawingSheetTextMeasurer.InkBounds(run.Text, run.Size, run.Family);
        return new Box(
            run.OriginX + ink.Left,
            run.BaselineY + ink.Top,
            run.OriginX + ink.Right,
            run.BaselineY + ink.Bottom);
    }

    private static FoundGlyphs? FindGlyphs(IReadOnlyList<PdfLetterInfo> letters, DrawingSheetPlacedText run)
    {
        const double selectTolerance = 1.5;
        var box = ExpectedInkBox(run);
        var picked = letters
            .Where(l => l.Text.Trim().Length > 0
                && l.GlyphCenterX >= box.Left - selectTolerance && l.GlyphCenterX <= box.Right + selectTolerance
                && l.GlyphCenterY >= box.Top - selectTolerance && l.GlyphCenterY <= box.Bottom + selectTolerance)
            .ToList();

        return picked.Count == 0
            ? null
            : new FoundGlyphs(
                new Box(
                    picked.Min(l => l.GlyphLeft),
                    picked.Min(l => l.GlyphTop),
                    picked.Max(l => l.GlyphRight),
                    picked.Max(l => l.GlyphBottom)),
                picked);
    }

    private static Box? FindExampleBox(IReadOnlyList<PdfWordInfo> words, DrawingSheetPlacedText run)
    {
        const double proximity = 6.0;
        var box = ExpectedInkBox(run);
        var normalizedRun = Normalize(run.Text);
        var matched = words
            .Where(w =>
            {
                var normalizedWord = Normalize(w.Text);
                if (normalizedWord.Length == 0 || !normalizedRun.Contains(normalizedWord, StringComparison.Ordinal))
                {
                    return false;
                }

                var centerX = (w.X0 + w.X1) / 2.0;
                var centerY = (w.Top + w.Bottom) / 2.0;
                return centerX >= box.Left - proximity && centerX <= box.Right + proximity
                    && centerY >= box.Top - proximity && centerY <= box.Bottom + proximity;
            })
            .ToList();

        return matched.Count == 0
            ? null
            : new Box(
                matched.Min(w => w.X0),
                matched.Min(w => w.Top),
                matched.Max(w => w.X1),
                matched.Max(w => w.Bottom));
    }

    private static string LongText(string seed, int length)
    {
        var text = seed;
        while (text.Length < length)
        {
            text += " detalhes";
        }

        return text[..length];
    }

    private static string Normalize(string value) => Regex.Replace(value, @"\s+", string.Empty);

    private readonly record struct Box(double Left, double Top, double Right, double Bottom)
    {
        public double CenterX => (Left + Right) / 2.0;

        public double CenterY => (Top + Bottom) / 2.0;
    }

    private sealed record FoundGlyphs(Box Box, IReadOnlyList<PdfLetterInfo> Letters);
}
