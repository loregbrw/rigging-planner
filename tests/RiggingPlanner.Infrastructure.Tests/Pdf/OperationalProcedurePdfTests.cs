using System.Text.RegularExpressions;
using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Domain.Services;
using RiggingPlanner.Infrastructure.Pdf;
using RiggingPlanner.Infrastructure.Tests.Fixtures;
using RiggingPlanner.Infrastructure.Tests.Pdf.Tools;
using UglyToad.PdfPig;

namespace RiggingPlanner.Infrastructure.Tests.Pdf;

/// <summary>
/// Stage-4 checks of the operational procedure against the example document
/// (Fixtures/reference-plan-2607036.pdf, pages 6 and 7): content fidelity to
/// OperationalProcedureService.ResolveContent, letter-level position agreement
/// with the declared exceptions, and the fixed two-page A4 structure.
/// </summary>
public sealed class OperationalProcedurePdfTests
{
    private const double TolerancePt = 1.0;
    private const double BulletTolerancePt = 1.5;
    private const int Stage = 4;

    [Fact]
    public void Draft_sections_match_ResolveContent_text()
    {
        var (plan, _) = TestPlans.Reference2607036();
        plan.OperationName = "Içamento Chiller";

        var content = OperationalProcedureService.ResolveContent(plan, null);
        var pdf = new OperationalProcedureRenderer().Render(plan, null);

        AssertContentMatches(pdf, content);
    }

    [Fact]
    public void Finalized_sections_use_the_snapshot_not_live_data()
    {
        var (plan, _) = TestPlans.Reference2607036();
        plan.OperationName = "Içamento Chiller";
        var snapshot = OperationalProcedureService.CreateSnapshot(plan);

        plan.CraneModel = "MD 99.999";
        plan.OperationName = "Operação Alterada Depois";
        plan.Status = "finalized";

        var content = OperationalProcedureService.ResolveContent(plan, snapshot);
        Assert.Equal(snapshot.Content, content);

        var pdf = new OperationalProcedureRenderer().Render(plan, snapshot);
        AssertContentMatches(pdf, content);

        var page1 = Norm(string.Concat(PdfTextReader.ReadLetterPages(pdf)[0].Letters.Select(l => l.Text)));
        Assert.Contains("MD45007", page1);
        Assert.DoesNotContain("MD99999", page1);
        Assert.Contains("IçamentoChiller", page1);
        Assert.DoesNotContain("OperaçãoAlteradaDepois", page1);
    }

    [Fact]
    public void Export_does_not_create_or_modify_a_snapshot()
    {
        var (plan, _) = TestPlans.Reference2607036();
        plan.OperationName = "Içamento Chiller";
        var before = OperationalProcedureService.GenerateContent(plan);

        _ = new OperationalProcedureRenderer().Render(plan, null);

        Assert.Null(plan.ProcedureSnapshot);
        Assert.Equal(before, OperationalProcedureService.GenerateContent(plan));
    }

    [Fact]
    public void Letters_match_example_pages_6_and_7_within_tolerance()
    {
        var (plan, _) = TestPlans.Reference2607036();
        plan.OperationName = "Içamento Chiller";
        var pdf = new OperationalProcedureRenderer().Render(plan, null);

        var examplePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "reference-plan-2607036.pdf");
        var exampleBytes = File.ReadAllBytes(examplePath);
        var example = PdfTextReader.ReadLetterPages(exampleBytes);
        var generated = PdfTextReader.ReadLetterPages(pdf);

        ReviewOutput.WriteFile(Stage, "procedimento.pdf", pdf);

        var exceptedTexts = BuildExceptedTexts(plan);
        var diffs = new List<string>();
        var metrics = new List<string>();

        for (var i = 0; i < 2; i++)
        {
            var examplePage = example[5 + i];
            var generatedPage = generated[i];

            var generatedPng = PdfRasterizer.RasterizeToPng(pdf, i);
            var examplePng = PdfRasterizer.RasterizeToPng(exampleBytes, 5 + i);
            var overlay = PdfOverlay.Compose(examplePng, generatedPng);
            ReviewOutput.WriteFile(Stage, $"overlay-page-{i + 1}.png", overlay.Png);
            metrics.Add($"page {i + 1}: overlay {overlay.DivergentPixels}/{overlay.TotalPixels} ({overlay.DivergencePercent:F3}%)");

            MatchPage(examplePage, generatedPage, exceptedTexts, diffs);
        }

        var report = new List<string> { $"letter diffs: {diffs.Count} (tolerance {TolerancePt}pt, bullets {BulletTolerancePt}pt)" };
        report.AddRange(diffs.Take(40));
        var reportPath = ReviewOutput.WriteText(Stage, "letter-diff-report.txt", string.Join(Environment.NewLine, report));
        ReviewOutput.WriteText(Stage, "diff-metrics.txt", string.Join(Environment.NewLine, metrics));

        Assert.True(diffs.Count == 0, $"{diffs.Count} letter diffs. See {reportPath}\n{string.Join("\n", diffs.Take(20))}");
    }

    [Fact]
    public void Procedure_is_two_A4_pages_with_correct_section_split_and_footer_numbers()
    {
        var (plan, _) = TestPlans.Reference2607036();
        plan.OperationName = "Içamento Chiller";
        var pdf = new OperationalProcedureRenderer().Render(plan, null);

        using var document = PdfDocument.Open(pdf);
        Assert.Equal(2, document.NumberOfPages);

        var pages = document.GetPages().ToList();
        foreach (var page in pages)
        {
            Assert.InRange((double)page.Width, ProcedureLayout.PageWidth - 0.5, ProcedureLayout.PageWidth + 0.5);
            Assert.InRange((double)page.Height, ProcedureLayout.PageHeight - 0.5, ProcedureLayout.PageHeight + 0.5);
        }

        var page1 = Norm(string.Concat(pages[0].Letters.Select(l => l.Value)));
        var page2 = Norm(string.Concat(pages[1].Letters.Select(l => l.Value)));

        Assert.Contains("1.ISOLAMENTODAÁREA", page1);
        Assert.Contains("2.PATOLAMENTODOGUINDASTEMD45007", page1);
        Assert.Contains("3.POSICIONAMENTODOVEÍCULODECARGA", page1);
        Assert.DoesNotContain("4.IÇAMENTODACARGA", page1);
        Assert.DoesNotContain("5.DESCARGADACARGA", page1);

        Assert.Contains("4.IÇAMENTODACARGA", page2);
        Assert.Contains("5.DESCARGADACARGA", page2);
        Assert.DoesNotContain("1.ISOLAMENTODAÁREA", page2);

        foreach (var pageText in new[] { page1, page2 })
        {
            Assert.Contains("Procedimento", pageText);
            Assert.Contains("ART", pageText);
            Assert.Contains("RIGGING", pageText);
            Assert.Contains("1720264496217", pageText);
            Assert.Contains("2607036", pageText);
            Assert.Contains(Norm(OperationalProcedureService.CompanyFooter), pageText);
        }

        Assert.Contains(pages[0].Letters, l => l.Value == "1" && l.Location.Y < 60 && l.Location.X > 540);
        Assert.Contains(pages[1].Letters, l => l.Value == "2" && l.Location.Y < 60 && l.Location.X > 540);
    }

    private static void AssertContentMatches(byte[] pdf, string content)
    {
        var sections = ParseSections(content);
        Assert.NotEmpty(sections);

        var generatedPages = PdfTextReader.ReadLetterPages(pdf);
        var pageNorms = generatedPages
            .Select(p => Norm(string.Concat(p.Letters.Select(l => l.Text))))
            .ToList();

        var lastIndexByPage = new Dictionary<int, int>();
        foreach (var section in sections)
        {
            var expected =
                Norm($"{section.Number}. {section.Title}")
                + Norm("Objetivo:") + Norm(section.Objective)
                + Norm("Passos:") + string.Concat(section.Steps.Select(Norm))
                + Norm("Referências:") + string.Concat(section.References.Select(Norm));

            var pageIdx = section.Page - 1;
            Assert.InRange(pageIdx, 0, pageNorms.Count - 1);
            var idx = pageNorms[pageIdx].IndexOf(expected, StringComparison.Ordinal);
            Assert.True(idx >= 0, $"Section {section.Number} text not found in order on page {section.Page}.");

            if (lastIndexByPage.TryGetValue(pageIdx, out var last))
            {
                Assert.True(idx > last, $"Section {section.Number} is out of order on page {section.Page}.");
            }
            lastIndexByPage[pageIdx] = idx;
        }
    }

    private static List<string> BuildExceptedTexts(RiggingPlan plan)
    {
        var sections = OperationalProcedureService.BuildDocument(plan).Pages.SelectMany(p => p.Sections).ToList();
        var sec2 = sections.First(s => s.Number == 2);
        var sec4 = sections.First(s => s.Number == 4);
        var sec5 = sections.First(s => s.Number == 5);

        return
        [
            Norm("Objetivo:" + sec4.Objective),
            Norm("Objetivo:" + sec5.Objective),
            Norm(sec2.References[^1]),
            Norm(sec4.References[^1]),
            Norm(sec4.References[0]),
        ];
    }

    private static void MatchPage(PdfPageLetters example, PdfPageLetters generated, List<string> exceptedTexts, List<string> diffs)
    {
        var exampleLetters = example.Letters.Where(l => l.Text.Trim().Length > 0).ToList();
        var exceptedBaselines = new HashSet<double>();
        foreach (var line in exampleLetters.GroupBy(l => Math.Round(l.Baseline, 1)))
        {
            var lineNorm = Norm(string.Concat(line.OrderBy(l => l.X).Select(l => l.Text)));
            if (exceptedTexts.Any(t => t.Contains(lineNorm)))
            {
                exceptedBaselines.Add(line.Key);
            }
        }

        var pool = generated.Letters
            .Where(l => l.Text.Trim().Length > 0)
            .Select(l => new LetterCandidate(l.Text, l.X, l.Baseline, l.Size))
            .ToList();

        foreach (var letter in exampleLetters)
        {
            var isBullet = letter.Text == "•";
            var isExcepted = exceptedBaselines.Contains(Math.Round(letter.Baseline, 1));
            var tolerance = isBullet ? BulletTolerancePt : TolerancePt;

            var best = -1;
            var bestDistance = double.MaxValue;
            for (var i = 0; i < pool.Count; i++)
            {
                var candidate = pool[i];
                if (candidate.Used || candidate.Text != letter.Text)
                {
                    continue;
                }

                double distance;
                if (isExcepted)
                {
                    if (Math.Abs(candidate.Baseline - letter.Baseline) > TolerancePt)
                    {
                        continue;
                    }
                    distance = Math.Abs(candidate.Baseline - letter.Baseline);
                }
                else if (isBullet)
                {
                    if (Math.Abs(candidate.Baseline - letter.Baseline) > BulletTolerancePt
                        || Math.Abs(candidate.X - letter.X) > BulletTolerancePt)
                    {
                        continue;
                    }
                    distance = Math.Abs(candidate.X - letter.X) + Math.Abs(candidate.Baseline - letter.Baseline);
                }
                else
                {
                    if (Math.Abs(candidate.Size - letter.Size) > 0.05
                        || Math.Abs(candidate.Baseline - letter.Baseline) > 2
                        || Math.Abs(candidate.X - letter.X) > 25)
                    {
                        continue;
                    }
                    distance = Math.Abs(candidate.X - letter.X) + Math.Abs(candidate.Baseline - letter.Baseline);
                }

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            if (best < 0)
            {
                diffs.Add($"UNMATCHED_EX\t{letter.Text}\tx={letter.X:F2}\tb={letter.Baseline:F2}\tsz={letter.Size:F2}");
                continue;
            }

            pool[best].Used = true;
            if (!isExcepted)
            {
                var dx = pool[best].X - letter.X;
                var db = pool[best].Baseline - letter.Baseline;
                if (Math.Abs(dx) > tolerance || Math.Abs(db) > tolerance)
                {
                    diffs.Add($"OFF\t{letter.Text}\tdx={dx:F2}\tdb={db:F2}\tex=({letter.X:F2},{letter.Baseline:F2})\tgen=({pool[best].X:F2},{pool[best].Baseline:F2})");
                }
            }
        }

        foreach (var extra in pool.Where(c => !c.Used))
        {
            diffs.Add($"UNMATCHED_GEN\t{extra.Text}\tx={extra.X:F2}\tb={extra.Baseline:F2}\tsz={extra.Size:F2}");
        }
    }

    private static List<ParsedSection> ParseSections(string content)
    {
        var sections = new List<ParsedSection>();
        ParsedSection? current = null;
        var page = 0;
        var mode = string.Empty;

        foreach (var raw in content.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0)
            {
                continue;
            }
            if (line == "Guindastes Ribas")
            {
                page++;
                continue;
            }

            var header = Regex.Match(line, @"^([1-5])\.\s*(.*)$");
            if (header.Success)
            {
                current = new ParsedSection { Page = page, Number = int.Parse(header.Groups[1].Value), Title = header.Groups[2].Value.Trim() };
                sections.Add(current);
                mode = string.Empty;
                continue;
            }
            if (current is null)
            {
                continue;
            }
            if (line.StartsWith("Objetivo:", StringComparison.Ordinal))
            {
                current.Objective = line["Objetivo:".Length..].Trim();
                mode = string.Empty;
            }
            else if (line == "Passos:")
            {
                mode = "steps";
            }
            else if (line == "Referências:")
            {
                mode = "refs";
            }
            else if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                var item = line[2..].Trim();
                if (mode == "steps")
                {
                    current.Steps.Add(item);
                }
                else if (mode == "refs")
                {
                    current.References.Add(item);
                }
            }
        }

        return sections;
    }

    private static string Norm(string value) => Regex.Replace(value, @"[\s•]", string.Empty);

    private sealed class ParsedSection
    {
        public int Page { get; init; }
        public int Number { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Objective { get; set; } = string.Empty;
        public List<string> Steps { get; } = [];
        public List<string> References { get; } = [];
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
