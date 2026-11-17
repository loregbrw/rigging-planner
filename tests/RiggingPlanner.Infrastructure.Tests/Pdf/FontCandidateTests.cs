using System.Globalization;
using System.Text;
using System.Text.Json;
using SkiaSharp;
using Xunit.Abstractions;

namespace RiggingPlanner.Infrastructure.Tests.Pdf;

/// <summary>
/// Stage 2A, step 1 (substitute fonts): measures the free font candidates against the
/// widths of the example drawing sheet (Century Gothic / Eras ITC Demi) and writes
/// artifacts/pdf-review/stage-2a/font-measures.txt and font-candidates.png.
/// The candidate font files are not committed (privacy/licensing review pending), so the
/// test is a no-op when artifacts/pdf-review/stage-2a/fonts is empty (CI).
/// Widths are summed glyph advances (no kerning), matching the stage-1 shaping decision.
/// </summary>
public sealed class FontCandidateTests(ITestOutputHelper output)
{
    private const double StampLeft = 56.5;
    private const double StampRight = 566.9;
    private const double StampTop = 662.39;
    private const double StampBottom = 813.59;
    private const double PreviewScale = 2.0;
    private const double ExampleDpi = 150.0;
    private const double MinimumMarginPt = 1.0;

    private static readonly FontCandidate[] GothicCandidates =
    [
        new("Montserrat", "Montserrat-Regular.ttf", "OFL 1.1", FontClass.Gothic),
        new("Questrial", "Questrial-Regular.ttf", "OFL 1.1", FontClass.Gothic),
        new("Didact Gothic", "DidactGothic-Regular.ttf", "OFL 1.1", FontClass.Gothic),
        new("Poppins", "Poppins-Regular.ttf", "OFL 1.1", FontClass.Gothic),
    ];

    private static readonly FontCandidate[] BrandCandidates =
    [
        new("Source Sans 3 SemiBold", "SourceSans3-SemiBold.ttf", "OFL 1.1", FontClass.Brand),
        new("Nunito Sans SemiBold", "NunitoSans-SemiBold.ttf", "OFL 1.1", FontClass.Brand),
        new("Open Sans SemiBold", "OpenSans-SemiBold.ttf", "OFL 1.1", FontClass.Brand),
    ];

    // Text specs: text, example size, font class, cell left/right (pt) and alignment.
    // Centered texts are measured against the cell; left-aligned ones keep the example x0.
    private static readonly TextSpec[] Specs =
    [
        new("COMPRIMENTO", 8, FontClass.Gothic, 311.6, 375.4, true),
        new("LARGURA", 8, FontClass.Gothic, 375.4, 439.1, true),
        new("ALTURA", 8, FontClass.Gothic, 439.1, 502.9, true),
        new("PESO", 8, FontClass.Gothic, 502.9, 566.9, true),
        new("[m]", 8, FontClass.Gothic, 311.6, 375.4, true),
        new("[kg]", 8, FontClass.Gothic, 502.9, 566.9, true),
        new("DESCRIÇÃO DA CARGA", 8, FontClass.Gothic, 56.5, 311.6, true),
        new("DESCRIÇÃO", 8, FontClass.Gothic, 56.5, 311.6, true),
        new("Chiller", 8, FontClass.Gothic, 56.5, 311.6, true),
        new("4,7", 8, FontClass.Gothic, 311.6, 375.4, true),
        new("1,95", 8, FontClass.Gothic, 375.4, 439.1, true),
        new("2,3", 8, FontClass.Gothic, 439.1, 502.9, true),
        new("3450", 8, FontClass.Gothic, 502.9, 566.9, true),
        new("CARGA BRUTA", 6, FontClass.Gothic, 211.4, 286.2, false),
        new("CAPACIDADE CARGA", 6, FontClass.Gothic, 211.4, 286.2, false),
        new("TAXA DE UTILIZAÇÃO", 6, FontClass.Gothic, 211.4, 286.2, false),
        new("UTILIZAÇÃO", 6, FontClass.Gothic, 211.4, 286.2, false),
        new("ESFORÇO NAS SAPATAS", 6, FontClass.Gothic, 211.4, 286.2, false),
        new("VELOC. MÁX VENTO", 6, FontClass.Gothic, 211.4, 286.2, false),
        new("3,75", 6, FontClass.Gothic, 286.2, 336.7, false),
        new("7,95", 6, FontClass.Gothic, 286.2, 336.7, false),
        new("47,1", 6, FontClass.Gothic, 286.2, 336.7, false),
        new("3,17", 6, FontClass.Gothic, 286.2, 336.7, false),
        new("7,0", 6, FontClass.Gothic, 286.2, 336.7, false),
        new("OPERAÇÃO:", 5.67, FontClass.Gothic, 336.7, 566.9, false),
        new("CLIENTE:", 5.67, FontClass.Gothic, 336.7, 530.0, false),
        new("A4", 11, FontClass.Gothic, 530.0, 566.9, true),
        new("ART:", 5.67, FontClass.Gothic, 211.4, 336.7, false),
        new("Nº Rigging:", 5.66, FontClass.Gothic, 211.4, 336.7, false),
        new("Revisão:", 5.67, FontClass.Gothic, 211.4, 336.7, false),
        new("1 de 1", 6, FontClass.Gothic, 211.4, 336.7, false),
        new("ESCALA", 5.67, FontClass.Gothic, 336.7, 462.0, false),
        new("Diversas", 5.67, FontClass.Gothic, 336.7, 462.0, false),
        new("FOLHA 1 DE 3", 5.67, FontClass.Gothic, 462.0, 566.7, false),
        new("RIGGER - RPP-23-1040", 7, FontClass.Gothic, 56.6, 136.9, true),
        new("CREA-PR 78755/D", 7, FontClass.Gothic, 136.9, 211.4, true),
        new("CNPJ 81.076.242/0001-60", 7, FontClass.Gothic, 56.6, 211.4, true),
        new("Rua Justo Manfron, 2845 G", 7, FontClass.Gothic, 56.6, 211.4, true),
        new("Santa Felicidade - Curitiba - PR", 7, FontClass.Gothic, 56.6, 211.4, true),
        new("CEP 82.410-540", 7, FontClass.Gothic, 56.6, 211.4, true),
        new("1720264496217", 9, FontClass.Gothic, 211.4, 336.7, true),
        new("2607036", 9, FontClass.Gothic, 211.4, 336.7, true),
        new("Içamento Chiller", 16, FontClass.Gothic, 336.7, 566.9, true),
        new("Kaefer Rip Ltda.", 16, FontClass.Gothic, 336.7, 530.0, true),
        new("4", 15, FontClass.Gothic, 56.5, 170.0, true),
        new("F", 15, FontClass.Gothic, 42.5, 56.5, true),
        new("E", 15, FontClass.Gothic, 42.5, 56.5, true),
        new("D", 15, FontClass.Gothic, 42.5, 56.5, true),
        new("C", 15, FontClass.Gothic, 42.5, 56.5, true),
        new("B", 15, FontClass.Gothic, 42.5, 56.5, true),
        new("A", 15, FontClass.Gothic, 42.5, 56.5, true),
        new("Guindastes Ribas Ltda.", 12, FontClass.Brand, 56.6, 211.4, true),
        new("Giuliano Breda", 10, FontClass.Brand, 78.8, 188.4, true),
    ];

    [Fact]
    public void Candidate_fonts_are_measured_and_previewed()
    {
        var stageDir = Path.Combine(ReviewOutputRoot(), "artifacts", "pdf-review", "stage-2a");
        var fontsDir = Path.Combine(stageDir, "fonts");

        var candidates = GothicCandidates.Concat(BrandCandidates).ToArray();
        var missing = candidates.Where(c => !File.Exists(Path.Combine(fontsDir, c.FileName))).ToArray();
        if (missing.Length > 0)
        {
            output.WriteLine(
                $"Candidate fonts not found in '{fontsDir}' (not committed by design). " +
                "Skipping the measurement; see docs/pdf-export/etapa-2a-relatorio.md for the download list.");
            return;
        }

        var referencePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "drawing-sheet.reference.json");
        Assert.True(File.Exists(referencePath), $"Reference fixture not found: {referencePath}");
        var sheet = LoadReference(referencePath);
        Assert.NotEmpty(sheet.Words);

        Directory.CreateDirectory(stageDir);

        var allText = string.Concat(sheet.Words.Select(w => w.Text).Distinct()) + " ";
        var report = new StringBuilder();
        var summary = new List<(FontCandidate Candidate, double MeanError, double MaxError, int Overflows)>();

        report.AppendLine("Medição das fontes candidatas — etapa 2A, Passo 1");
        report.AppendLine("Referência: Fixtures/drawing-sheet.reference.json (larguras do exemplo)");
        report.AppendLine("Método: largura da caixa de tinta do texto (sem kerning) no tamanho do exemplo, em pt.");
        report.AppendLine($"Data: {DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}");
        report.AppendLine();

        foreach (var fontClass in new[] { FontClass.Gothic, FontClass.Brand })
        {
            var classCandidates = candidates.Where(c => c.Class == fontClass).ToArray();
            var classSpecs = Specs.Where(s => s.Class == fontClass).ToArray();
            report.AppendLine(fontClass == FontClass.Gothic
                ? "Fonte gótica (substituta de Century Gothic)"
                : "Fonte \"marca\" (substituta de Eras ITC Demi)");
            report.AppendLine(new string('=', 60));

            foreach (var candidate in classCandidates)
            {
                var path = Path.Combine(fontsDir, candidate.FileName);
                using var typeface = OpenTypeface(path);
                using var font = new SKFont(typeface, 1);
                var coverage = font.ContainsGlyphs(allText) ? "OK" : "FALTAM GLIFOS";

                var rows = new List<MeasureRow>();
                foreach (var spec in classSpecs)
                {
                    var example = FindRun(sheet.Words, spec.Text, spec.Size)
                        ?? throw new InvalidOperationException($"Example run not found for '{spec.Text}' at {spec.Size} pt.");
                    var width = MeasureWidth(typeface, spec.Text, spec.Size);
                    var exampleWidth = example.X1 - example.X0;
                    var relative = exampleWidth > 0 ? (width - exampleWidth) / exampleWidth : 0;
                    var margins = FitMargins(spec, example, width);
                    rows.Add(new MeasureRow(spec, example, exampleWidth, width, relative, margins));
                }

                var meanError = rows.Average(r => Math.Abs(r.RelativeError));
                var maxError = rows.Max(r => Math.Abs(r.RelativeError));
                var overflows = rows.Where(r => !r.Fits).ToArray();
                summary.Add((candidate, meanError, maxError, overflows.Length));

                report.AppendLine($"{candidate.Label} ({candidate.FileName}) — licença {candidate.License}");
                report.AppendLine($"  cobertura de glifos: {coverage}");
                report.AppendLine(
                    $"  erro relativo médio |Δ|: {Abs(meanError)}   máximo: {Abs(maxError)}   " +
                    $"textos que estouram a célula: {(overflows.Length == 0 ? "nenhum" : overflows.Length.ToString(CultureInfo.InvariantCulture))}");
                report.AppendLine("  texto                              tam    exemplo   medido     erro      célula    cabe");
                foreach (var row in rows)
                {
                    var cell = row.Spec.CellRight - row.Spec.CellLeft;
                    report.AppendLine(
                        $"  {row.Spec.Text,-32} {Num(row.Spec.Size),6} {Num(row.ExampleWidth),9} {Num(row.Width),9} " +
                        $"{Signed(row.RelativeError),9} {Num(cell),9} {(row.Fits ? "sim" : "NÃO"),5}");
                }

                if (overflows.Length > 0)
                {
                    report.AppendLine("  ESTOUROS (proposta: reduzir 0,5 pt e remedir):");
                    foreach (var row in overflows)
                    {
                        var reduced = MeasureWidth(typeface, row.Spec.Text, row.Spec.Size - 0.5);
                        var reducedMargins = FitMargins(row.Spec, row.Example, reduced);
                        report.AppendLine(
                            $"    {row.Spec.Text,-32} margens {Num(row.Margins.Left)}/{Num(row.Margins.Right)} pt " +
                            $"→ a {Num(row.Spec.Size - 0.5)} pt: {Num(reduced)} pt, cabe: {(reducedMargins.Fits ? "sim" : "não")}");
                    }
                }

                report.AppendLine();
            }
        }

        report.AppendLine("Resumo (erro relativo médio |Δ|)");
        report.AppendLine(new string('=', 60));
        foreach (var (candidate, meanError, maxError, overflows) in summary)
        {
            report.AppendLine(
                $"{candidate.Label,-26} {Abs(meanError),8}   máx {Abs(maxError),8}   estouros: {overflows}");
        }

        report.AppendLine();
        report.AppendLine("Textos mínimos exigidos pela especificação (seção 3):");
        foreach (var text in new[]
                 {
                     "COMPRIMENTO", "DESCRIÇÃO", "UTILIZAÇÃO", "Içamento Chiller",
                     "Kaefer Rip Ltda.", "Rua Justo Manfron, 2845 G", "Guindastes Ribas Ltda.", "Giuliano Breda",
                 })
        {
            report.AppendLine($"  {text}");
        }

        File.WriteAllText(Path.Combine(stageDir, "font-measures.txt"), report.ToString());
        File.WriteAllBytes(Path.Combine(stageDir, "font-candidates.png"), RenderPreview(sheet, fontsDir, referencePath));
        output.WriteLine($"Artifacts written to {stageDir}");
    }

    private static string ReviewOutputRoot() => Tools.ReviewOutput.RootDirectory();

    private static RefSheet LoadReference(string path)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<RefSheet>(File.ReadAllText(path), options)
            ?? throw new InvalidOperationException("Could not deserialize the drawing-sheet reference JSON.");
    }

    private static SKTypeface OpenTypeface(string path) =>
        SKTypeface.FromFile(path) ?? throw new InvalidOperationException($"Could not open typeface: {path}");

    private static double MeasureWidth(SKTypeface typeface, string text, double sizePt)
    {
        // The example widths are glyph bounding boxes (x1 - x0), so compare ink to ink.
        using var font = new SKFont(typeface, (float)sizePt);
        using var paint = new SKPaint();
        font.MeasureText(text, out var bounds, paint);
        return bounds.Width;
    }

    private static (double Left, double Right, bool Fits) FitMargins(TextSpec spec, RefRun example, double width)
    {
        double left;
        double right;
        if (spec.Centered)
        {
            var available = spec.CellRight - spec.CellLeft;
            left = right = (available - width) / 2;
        }
        else
        {
            left = example.X0 - spec.CellLeft;
            right = spec.CellRight - (example.X0 + width);
        }

        return (left, right, left >= MinimumMarginPt && right >= MinimumMarginPt);
    }

    private static RefRun? FindRun(IReadOnlyList<RefWord> words, string text, double size)
    {
        var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i + tokens.Length <= words.Count; i++)
        {
            var first = words[i];
            if (!TokenMatches(first.Text, tokens[0]) || Math.Abs(first.Size - size) > 0.01)
            {
                continue;
            }

            var ok = true;
            var lastX1 = first.X1;
            for (var k = 0; k < tokens.Length; k++)
            {
                var word = words[i + k];
                if (!TokenMatches(word.Text, tokens[k]) || Math.Abs(word.Size - size) > 0.01 ||
                    Math.Abs(word.Bottom - first.Bottom) > 0.5)
                {
                    ok = false;
                    break;
                }

                if (k > 0 && (word.X0 < lastX1 - 0.5 || word.X0 - lastX1 > 0.5 * word.Size))
                {
                    ok = false;
                    break;
                }

                lastX1 = word.X1;
            }

            if (ok)
            {
                return new RefRun(text, first.X0, lastX1, first.Top, words[i + tokens.Length - 1].Bottom, first.Font, first.Size);
            }
        }

        return null;
    }

    private static bool TokenMatches(string actual, string expected) =>
        string.Equals(actual.Trim(), expected.Trim(), StringComparison.Ordinal);

    private static string Num(double value) => value.ToString("0.00", CultureInfo.GetCultureInfo("pt-BR"));

    private static string Abs(double value) => (value * 100).ToString("0.00", CultureInfo.GetCultureInfo("pt-BR")) + "%";

    private static string Signed(double value) => (value * 100).ToString("+0.00;-0.00;0.00", CultureInfo.GetCultureInfo("pt-BR")) + "%";

    private static byte[] RenderPreview(RefSheet sheet, string fontsDir, string referencePath)
    {
        var stampWidth = (float)((StampRight - StampLeft) * PreviewScale);
        var stampHeight = (float)((StampBottom - StampTop) * PreviewScale);
        const float labelWidth = 360;
        const float labelHeight = 26;
        const float gap = 12;

        using var gothicFixed = OpenTypeface(Path.Combine(fontsDir, GothicCandidates[0].FileName));
        using var brandFixed = OpenTypeface(Path.Combine(fontsDir, BrandCandidates[0].FileName));

        var opened = new List<SKTypeface>();
        var rows = new List<(string Label, SKTypeface? Gothic, SKTypeface? Brand, bool IsExample)>
        {
            ("EXEMPLO (Century Gothic / Eras ITC Demi)", null, null, true),
        };

        foreach (var candidate in GothicCandidates)
        {
            var typeface = OpenTypeface(Path.Combine(fontsDir, candidate.FileName));
            opened.Add(typeface);
            rows.Add(($"GÓTICA: {candidate.Label} — marca fixa: {BrandCandidates[0].Label}", typeface, brandFixed, false));
        }

        foreach (var candidate in BrandCandidates)
        {
            var typeface = OpenTypeface(Path.Combine(fontsDir, candidate.FileName));
            opened.Add(typeface);
            rows.Add(($"MARCA: {candidate.Label} — gótica fixa: {GothicCandidates[0].Label}", gothicFixed, typeface, false));
        }

        var width = (int)Math.Ceiling(labelWidth + stampWidth + 24);
        var rowHeight = (int)Math.Ceiling(labelHeight + stampHeight + gap);
        var height = rowHeight * rows.Count + 12;
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        using var labelTypeface = SKTypeface.FromFamilyName("Segoe UI") ?? SKTypeface.CreateDefault();
        using var labelFont = new SKFont(labelTypeface, 16);
        using var labelPaint = new SKPaint { Color = new SKColor(40, 40, 40), IsAntialias = true };
        using var textPaint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        using var linePaint = new SKPaint
        {
            Color = new SKColor(192, 192, 192),
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
        };
        using var borderPaint = new SKPaint
        {
            Color = new SKColor(170, 170, 170),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true,
        };
        using var imagePaint = new SKPaint { IsAntialias = true };

        using var examplePage = LoadExampleStamp(referencePath);
        var exampleSource = new SKRect(
            (float)(StampLeft * ExampleDpi / 72.0),
            (float)(StampTop * ExampleDpi / 72.0),
            (float)(StampRight * ExampleDpi / 72.0),
            (float)(StampBottom * ExampleDpi / 72.0));

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var top = 12 + index * rowHeight;
            canvas.DrawText(row.Label, 8, top + 18, SKTextAlign.Left, labelFont, labelPaint);

            var destination = new SKRect(labelWidth + 12, top + labelHeight, labelWidth + 12 + stampWidth, top + labelHeight + stampHeight);
            canvas.DrawRect(destination, borderPaint);

            if (row.IsExample)
            {
                canvas.DrawBitmap(examplePage, exampleSource, destination, new SKSamplingOptions(SKFilterMode.Linear), imagePaint);
                continue;
            }

            canvas.Save();
            canvas.ClipRect(destination);
            canvas.Translate(destination.Left, destination.Top);
            DrawStampLines(canvas, sheet, linePaint);
            DrawStampWords(canvas, sheet, row.Gothic!, row.Brand!, textPaint);
            canvas.Restore();
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static SKBitmap LoadExampleStamp(string referencePath)
    {
        var pdfPath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "reference-plan-2607036.pdf");
        Assert.True(File.Exists(pdfPath), $"Example PDF not found: {pdfPath}");
        var png = Tools.PdfRasterizer.RasterizeToPng(File.ReadAllBytes(pdfPath), page: 1, dpi: (int)ExampleDpi);
        return SKBitmap.Decode(png);
    }

    private static void DrawStampLines(SKCanvas canvas, RefSheet sheet, SKPaint paint)
    {
        foreach (var line in sheet.Lines)
        {
            var x0 = Math.Max(line.X0, StampLeft);
            var x1 = Math.Min(line.X1, StampRight);
            var y0 = Math.Max(line.Top, StampTop);
            var y1 = Math.Min(line.Bottom, StampBottom);
            if (x0 > x1 || y0 > y1)
            {
                continue;
            }

            paint.StrokeWidth = (float)(Math.Max(line.Lw, 0.3) * PreviewScale);
            canvas.DrawLine(
                (float)((x0 - StampLeft) * PreviewScale),
                (float)((y0 - StampTop) * PreviewScale),
                (float)((x1 - StampLeft) * PreviewScale),
                (float)((y1 - StampTop) * PreviewScale),
                paint);
        }
    }

    private static void DrawStampWords(SKCanvas canvas, RefSheet sheet, SKTypeface gothic, SKTypeface brand, SKPaint paint)
    {
        foreach (var run in GroupWords(sheet.Words))
        {
            if (run.Bottom < StampTop || run.Top > StampBottom || run.X1 < StampLeft || run.X0 > StampRight)
            {
                continue;
            }

            var isBrand = run.Font.Contains("Eras", StringComparison.OrdinalIgnoreCase);
            var typeface = isBrand ? brand : gothic;
            var size = (float)(run.Size * PreviewScale);
            using var font = new SKFont(typeface, size);
            font.MeasureText(run.Text, out var bounds, paint);

            var spec = Specs.FirstOrDefault(s => s.Class == (isBrand ? FontClass.Brand : FontClass.Gothic) &&
                                                 string.Equals(s.Text, run.Text, StringComparison.Ordinal));
            var centerX = (float)(((run.X0 + run.X1) / 2 - StampLeft) * PreviewScale);
            var leftX = (float)((run.X0 - StampLeft) * PreviewScale);
            var originX = spec is { Centered: true }
                ? centerX - (bounds.Left + bounds.Right) / 2
                : leftX - bounds.Left;

            var exampleCenterY = (float)(((run.Top + run.Bottom) / 2 - StampTop) * PreviewScale);
            var baseline = exampleCenterY - (bounds.Top + bounds.Bottom) / 2;
            canvas.DrawText(run.Text, originX, baseline, SKTextAlign.Left, font, paint);
        }
    }

    /// <summary>
    /// Rebuilds the example text runs (words separated by the example's normal word gaps)
    /// so the preview draws whole strings with the candidate font's own spacing.
    /// </summary>
    private static List<RefRun> GroupWords(IReadOnlyList<RefWord> words)
    {
        var runs = new List<RefRun>();
        foreach (var word in words)
        {
            if (runs.Count > 0)
            {
                var last = runs[^1];
                var sameLine = Math.Abs(last.Bottom - word.Bottom) <= 0.5 && Math.Abs(last.Top - word.Top) <= 0.5;
                var sameFont = string.Equals(last.Font, word.Font, StringComparison.Ordinal) &&
                               Math.Abs(last.Size - word.Size) <= 0.01;
                var gap = word.X0 - last.X1;
                if (sameLine && sameFont && gap >= -0.5 && gap <= 0.30 * word.Size)
                {
                    runs[^1] = last with { Text = last.Text + " " + word.Text, X1 = word.X1 };
                    continue;
                }
            }

            runs.Add(new RefRun(word.Text, word.X0, word.X1, word.Top, word.Bottom, word.Font, word.Size));
        }

        return runs;
    }

    private enum FontClass
    {
        Gothic,
        Brand,
    }

    private sealed record FontCandidate(string Label, string FileName, string License, FontClass Class);

    private sealed record TextSpec(string Text, double Size, FontClass Class, double CellLeft, double CellRight, bool Centered);

    private sealed record RefRun(string Text, double X0, double X1, double Top, double Bottom, string Font, double Size);

    private sealed record MeasureRow(
        TextSpec Spec,
        RefRun Example,
        double ExampleWidth,
        double Width,
        double RelativeError,
        (double Left, double Right, bool Fits) Margins)
    {
        public bool Fits => Margins.Fits;
    }

    private sealed class RefSheet
    {
        public List<RefLine> Lines { get; set; } = [];

        public List<RefWord> Words { get; set; } = [];
    }

    private sealed class RefLine
    {
        public double X0 { get; set; }

        public double X1 { get; set; }

        public double Top { get; set; }

        public double Bottom { get; set; }

        public double Lw { get; set; }
    }

    private sealed class RefWord
    {
        public string Text { get; set; } = string.Empty;

        public double X0 { get; set; }

        public double X1 { get; set; }

        public double Top { get; set; }

        public double Bottom { get; set; }

        public string Font { get; set; } = string.Empty;

        public double Size { get; set; }
    }
}
