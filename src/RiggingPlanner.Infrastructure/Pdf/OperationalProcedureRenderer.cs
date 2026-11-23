using System.Text.RegularExpressions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RiggingPlanner.Application.Pdf;
using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Domain.Services;

namespace RiggingPlanner.Infrastructure.Pdf;

/// <summary>
/// Renderer for the operational procedure (2 fixed A4 pages) using QuestPDF.
/// Draws exactly the content returned by OperationalProcedureService.ResolveContent
/// at the metric positions measured from the reference plan (ProcedureLayout).
/// </summary>
public sealed class OperationalProcedureRenderer : IOperationalProcedureRenderer
{
    private const string LogoResourceName = "RiggingPlanner.Infrastructure.Pdf.Images.guindastes-ribas-logo.png";

    public byte[] Render(RiggingPlan plan, PlanProcedureSnapshot? snapshot)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var content = OperationalProcedureService.ResolveContent(plan, snapshot);
        return RenderFromContent(content);
    }

    public byte[] RenderFromContent(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        PdfFontRegistry.EnsureRegistered();

        var document = ProcedureContentParser.Parse(content);
        var logo = LoadLogo();

        return Document.Create(container =>
        {
            var pageIndex = 0;
            foreach (var pageModel in document.Pages)
            {
                pageIndex++;
                var procedurePageNum = pageModel.Number > 0 ? pageModel.Number : pageIndex;

                container.Page(page =>
                {
                    page.Size(new PageSize((float)ProcedureLayout.PageWidth, (float)ProcedureLayout.PageHeight, Unit.Point));
                    page.Margin(0);

                    page.DefaultTextStyle(text => text
                        .FontFamily(PdfFontRegistry.FamilyName)
                        .DisableFontFeature(FontFeatures.Kerning)
                        .DisableFontFeature(FontFeatures.StandardLigatures));

                    var runs = BuildRuns(document, pageModel, procedurePageNum, out var separatorTops);
                    var rects = BuildRects(separatorTops);

                    page.Content().Layers(layers =>
                    {
                        layers.PrimaryLayer().Element(c => ComposeRect(c, rects[0]));
                        foreach (var rect in rects.Skip(1))
                        {
                            layers.Layer().Element(c => ComposeRect(c, rect));
                        }

                        layers.Layer().Element(c => ComposeLogo(c, logo));
                        layers.Layer().Element(c => ComposeRuns(c, runs));
                    });
                });
            }
        }).GeneratePdf();
    }

    private static List<ProcedureRect> BuildRects(List<double> separatorTops)
    {
        var rects = new List<ProcedureRect>
        {
            new(ProcedureLayout.VerticalLine1X, ProcedureLayout.VerticalLine1X + ProcedureLayout.VerticalLineWidth, ProcedureLayout.VerticalLineY1, ProcedureLayout.VerticalLineY2, ProcedureLayout.VerticalLineColor),
            new(ProcedureLayout.VerticalLine2X, ProcedureLayout.VerticalLine2X + ProcedureLayout.VerticalLineWidth, ProcedureLayout.VerticalLineY1, ProcedureLayout.VerticalLineY2, ProcedureLayout.VerticalLineColor),
            new(ProcedureLayout.RightHeaderLineX1, ProcedureLayout.RightHeaderLineX2, ProcedureLayout.RightHeaderLineY1, ProcedureLayout.RightHeaderLineY2, ProcedureLayout.RightHeaderLineColor),
            new(ProcedureLayout.HeaderBottomLineX1, ProcedureLayout.HeaderBottomLineSplitX, ProcedureLayout.HeaderBottomLineY1, ProcedureLayout.HeaderBottomLineY2, ProcedureLayout.HeaderBottomLineLeftColor),
            new(ProcedureLayout.RightHeaderLineX1, ProcedureLayout.RightHeaderLineX2, ProcedureLayout.HeaderBottomLineY1, ProcedureLayout.HeaderBottomLineY2, ProcedureLayout.HeaderBottomLineRightColor),
            new(ProcedureLayout.FooterLineX1, ProcedureLayout.FooterLineX2, ProcedureLayout.FooterLineY, ProcedureLayout.FooterLineY + ProcedureLayout.FooterLineWidth, ProcedureLayout.FooterLineColor)
        };

        foreach (var top in separatorTops)
        {
            rects.Add(new(ProcedureLayout.SeparatorX1, ProcedureLayout.SeparatorX2, top, top + ProcedureLayout.SeparatorThickness, ProcedureLayout.SeparatorColor));
        }

        return rects;
    }

    private static List<ProcedureRun> BuildRuns(ParsedProcedureDocument document, ParsedProcedurePage page, int pageNum, out List<double> separatorTops)
    {
        var runs = new List<ProcedureRun>();
        separatorTops = [];

        runs.Add(new ProcedureRun("Procedimento", ProcedureLayout.CenterHeaderX, ProcedureLayout.ProcedimentoTitleBaseline, ProcedureLayout.ProcedimentoTitleSize, bold: true, family: PdfFontRegistry.SansFamilyName, align: ProcedureAlign.Center));
        AddOperationNameRuns(runs, document.OperationName);

        runs.Add(new ProcedureRun("ART", ProcedureLayout.RightHeaderCenterX, ProcedureLayout.ArtLabelBaseline, ProcedureLayout.ArtLabelSize, bold: false, family: PdfFontRegistry.SansFamilyName, align: ProcedureAlign.Center));
        runs.Add(new ProcedureRun(document.ArtNumber, ProcedureLayout.RightHeaderCenterX, ProcedureLayout.ArtValueBaseline, ProcedureLayout.ArtValueSize, bold: true, family: PdfFontRegistry.SansFamilyName, align: ProcedureAlign.Center));
        runs.Add(new ProcedureRun("RIGGING", ProcedureLayout.RightHeaderCenterX, ProcedureLayout.RiggingLabelBaseline, ProcedureLayout.RiggingLabelSize, bold: false, family: PdfFontRegistry.SansFamilyName, align: ProcedureAlign.Center));
        runs.Add(new ProcedureRun(document.PlanNumber, ProcedureLayout.RightHeaderCenterX, ProcedureLayout.RiggingValueBaseline, ProcedureLayout.RiggingValueSize, bold: true, family: PdfFontRegistry.SansFamilyName, align: ProcedureAlign.Center));

        runs.Add(new ProcedureRun(OperationalProcedureService.CompanyFooter, ProcedureLayout.FooterTextX, ProcedureLayout.FooterTextBaseline, ProcedureLayout.FooterTextSize, false, PdfFontRegistry.FamilyName, ProcedureAlign.Left, ProcedureLayout.FooterTextColor));
        runs.Add(new ProcedureRun(pageNum.ToString(), ProcedureLayout.FooterPageNumberRightX, ProcedureLayout.FooterPageNumberBaseline, ProcedureLayout.FooterPageNumberSize, false, PdfFontRegistry.FamilyName, ProcedureAlign.Right, ProcedureLayout.FooterTextColor));

        foreach (var section in page.Sections)
        {
            var baseBaseline = section.Number switch
            {
                1 => ProcedureLayout.Section1TitleBaseline,
                2 => ProcedureLayout.Section2TitleBaseline,
                3 => ProcedureLayout.Section3TitleBaseline,
                4 => ProcedureLayout.Section4TitleBaseline,
                5 => ProcedureLayout.Section5TitleBaseline,
                _ => ProcedureLayout.Section1TitleBaseline
            };

            runs.Add(new ProcedureRun($"{section.Number}. {section.Title}", ProcedureLayout.BodyLeftX, baseBaseline, ProcedureLayout.SectionTitleSize, bold: true, family: PdfFontRegistry.FamilyName));

            var objBaseline = baseBaseline + ProcedureLayout.DeltaTitleToObjectiveBaseline;
            runs.Add(new ProcedureRun("Objetivo:", ProcedureLayout.BodyLeftX, objBaseline, ProcedureLayout.ObjectiveLabelSize, bold: true, family: PdfFontRegistry.FamilyName));
            var objTextX = ProcedureLayout.BodyLeftX
                + CarlitoMeasurer.MeasureWidth("Objetivo:", ProcedureLayout.ObjectiveLabelSize, bold: true)
                + MeasureSpace(ProcedureLayout.ObjectiveTextSize, bold: false);
            runs.Add(new ProcedureRun(section.Objective, objTextX, objBaseline, ProcedureLayout.ObjectiveTextSize, bold: false, family: PdfFontRegistry.FamilyName));

            var passosBaseline = baseBaseline + ProcedureLayout.DeltaTitleToPassosBaseline;
            runs.Add(new ProcedureRun("Passos:", ProcedureLayout.BodyLeftX, passosBaseline, ProcedureLayout.SubheadLabelSize, bold: true, family: PdfFontRegistry.FamilyName));

            var stepBaseline = baseBaseline + ProcedureLayout.DeltaTitleToFirstStepBaseline;
            foreach (var step in section.Steps)
            {
                var lines = LayoutItem(runs, step, stepBaseline, isReference: false);
                stepBaseline += lines * ProcedureLayout.StepLineAdvance;
            }

            var lastStepBaseline = stepBaseline - ProcedureLayout.StepLineAdvance;
            var refsBaseline = lastStepBaseline + ProcedureLayout.DeltaLastStepToReferencias;
            runs.Add(new ProcedureRun("Referências:", ProcedureLayout.BodyLeftX, refsBaseline, ProcedureLayout.SubheadLabelSize, bold: true, family: PdfFontRegistry.FamilyName));

            var refBaseline = refsBaseline + ProcedureLayout.DeltaReferenciasToFirstRef;
            foreach (var reference in section.References)
            {
                var lines = LayoutItem(runs, reference, refBaseline, isReference: true);
                refBaseline += lines * ProcedureLayout.RefLineAdvance;
            }

            var lastRefBaseline = refBaseline - ProcedureLayout.RefLineAdvance;
            if (section.Number != 5)
            {
                separatorTops.Add(lastRefBaseline - ProcedureLayout.ItemInkAscent + ProcedureLayout.SeparatorDeltaAfterLastRef);
            }
        }

        return runs;
    }

    private static void AddOperationNameRuns(List<ProcedureRun> runs, string opName)
    {
        const double maxW = 270.0;
        var chosenSize = ProcedureLayout.OperationNameSize;

        while (chosenSize >= ProcedureLayout.OperationNameMinSize)
        {
            var w = CarlitoMeasurer.MeasureWidth(opName, chosenSize, bold: true, family: PdfFontRegistry.SansFamilyName);
            if (w <= maxW)
            {
                runs.Add(new ProcedureRun(opName, ProcedureLayout.OperationNameCenterX, ProcedureLayout.OperationNameBaseline, chosenSize, bold: true, family: PdfFontRegistry.SansFamilyName, align: ProcedureAlign.Center));
                return;
            }
            chosenSize -= 0.5;
        }

        var words = opName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var half = words.Length / 2;
        var line1 = string.Join(" ", words.Take(half));
        var line2 = string.Join(" ", words.Skip(half));
        var halfAdvance = ProcedureLayout.OperationNameLineAdvance / 2.0;

        runs.Add(new ProcedureRun(line1, ProcedureLayout.OperationNameCenterX, ProcedureLayout.OperationNameBaseline - halfAdvance, ProcedureLayout.OperationNameMinSize, bold: true, family: PdfFontRegistry.SansFamilyName, align: ProcedureAlign.Center));
        runs.Add(new ProcedureRun(line2, ProcedureLayout.OperationNameCenterX, ProcedureLayout.OperationNameBaseline + halfAdvance, ProcedureLayout.OperationNameMinSize, bold: true, family: PdfFontRegistry.SansFamilyName, align: ProcedureAlign.Center));
    }

    private static int LayoutItem(List<ProcedureRun> runs, string text, double firstBaseline, bool isReference)
    {
        var words = TokenizeItem(text, isReference);
        if (words.Count == 0)
        {
            return 1;
        }

        const double size = ProcedureLayout.ItemTextSize;
        var spaceW = MeasureSpace(size, bold: false);
        var available = ProcedureLayout.BodyRightX - ProcedureLayout.ItemTextX;
        var totalWordW = words.Sum(w => w.Width);
        var gaps = words.Count - 1;
        var naturalW = totalWordW + spaceW * gaps;

        List<List<ItemWord>> lines;
        if (naturalW <= available || gaps == 0)
        {
            lines = [words];
        }
        else if (totalWordW <= available && naturalW - available <= ProcedureLayout.ItemMaxSpaceCompression * gaps)
        {
            lines = [words];
        }
        else
        {
            lines = WrapWords(words, available, spaceW);
        }

        var baseline = firstBaseline;
        for (var li = 0; li < lines.Count; li++)
        {
            var line = lines[li];
            var lineWordW = line.Sum(w => w.Width);
            var lineGaps = line.Count - 1;
            var isLastLine = li == lines.Count - 1;

            double space;
            if (lines.Count == 1)
            {
                space = naturalW <= available || lineGaps == 0
                    ? spaceW
                    : (available - lineWordW) / lineGaps;
            }
            else
            {
                space = isLastLine || lineGaps == 0
                    ? spaceW
                    : (available - lineWordW) / lineGaps;
            }

            if (li == 0)
            {
                runs.Add(new ProcedureRun("•", ProcedureLayout.BulletX, baseline, ProcedureLayout.ItemBulletSize, bold: false, family: PdfFontRegistry.FamilyName));
            }

            var x = ProcedureLayout.ItemTextX;
            foreach (var word in line)
            {
                runs.Add(new ProcedureRun(word.Text, x, baseline, size, word.Bold, PdfFontRegistry.FamilyName));
                x += word.Width + space;
            }

            baseline += ProcedureLayout.RefLineAdvance;
        }

        return lines.Count;
    }

    private static List<ItemWord> TokenizeItem(string text, bool isReference)
    {
        var dashIndex = isReference ? text.IndexOf(" – ", StringComparison.Ordinal) : -1;
        var words = new List<ItemWord>();
        var i = 0;
        while (i < text.Length)
        {
            if (text[i] == ' ')
            {
                i++;
                continue;
            }
            var start = i;
            while (i < text.Length && text[i] != ' ')
            {
                i++;
            }
            var word = text[start..i];
            var bold = dashIndex > 0 && start < dashIndex;
            var width = CarlitoMeasurer.MeasureWidth(word, ProcedureLayout.ItemTextSize, bold);
            words.Add(new ItemWord(word, bold, width));
        }
        return words;
    }

    private static List<List<ItemWord>> WrapWords(List<ItemWord> words, double available, double spaceW)
    {
        var lines = new List<List<ItemWord>>();
        var current = new List<ItemWord>();
        var width = 0.0;
        foreach (var word in words)
        {
            var add = current.Count == 0 ? word.Width : spaceW + word.Width;
            if (current.Count > 0 && width + add > available)
            {
                lines.Add(current);
                current = [];
                width = 0;
                add = word.Width;
            }
            current.Add(word);
            width += add;
        }
        if (current.Count > 0)
        {
            lines.Add(current);
        }
        return lines;
    }

    private static double MeasureSpace(double sizePt, bool bold) =>
        CarlitoMeasurer.MeasureWidth("a a", sizePt, bold) - CarlitoMeasurer.MeasureWidth("aa", sizePt, bold);

    private static void ComposeRuns(IContainer container, List<ProcedureRun> runs)
    {
        container.Layers(layers =>
        {
            layers.PrimaryLayer().Element(c => ComposeRun(c, runs[0]));
            foreach (var run in runs.Skip(1))
            {
                layers.Layer().Element(c => ComposeRun(c, run));
            }
        });
    }

    private static IContainer ComposeRun(IContainer container, ProcedureRun run)
    {
        var ascent = Ascent(run.Size, run.Bold, run.Family);
        var yTop = run.Baseline - ascent;

        var x = run.Align switch
        {
            ProcedureAlign.Left => run.X,
            ProcedureAlign.Center => run.X - CarlitoMeasurer.MeasureWidth(run.Text, run.Size, run.Bold, run.Family) / 2.0,
            ProcedureAlign.Right => run.X - CarlitoMeasurer.MeasureWidth(run.Text, run.Size, run.Bold, run.Family),
            _ => run.X
        };

        var result = container
            .PaddingTop((float)yTop)
            .PaddingLeft((float)x)
            .Unconstrained();

        result.Text(text =>
        {
            var span = text.Span(run.Text)
                .FontFamily(run.Family)
                .FontSize((float)run.Size);

            if (run.Bold)
            {
                span.Bold();
            }

            if (!string.IsNullOrEmpty(run.Color))
            {
                span.FontColor(run.Color);
            }
        });

        return result;
    }

    private static IContainer ComposeRect(IContainer container, ProcedureRect rect)
    {
        var result = container
            .PaddingTop((float)rect.Y1)
            .PaddingLeft((float)rect.X1)
            .Width((float)(rect.X2 - rect.X1))
            .Height((float)(rect.Y2 - rect.Y1));

        result.Background(rect.Color);
        return result;
    }

    private static IContainer ComposeLogo(IContainer container, byte[] logo)
    {
        var (imageWidth, imageHeight) = PngSize(logo);
        var boxWidth = ProcedureLayout.LogoBoxX2 - ProcedureLayout.LogoBoxX1;
        var boxHeight = ProcedureLayout.LogoBoxY2 - ProcedureLayout.LogoBoxY1;
        var ratio = (double)imageWidth / imageHeight;

        double renderedWidth, renderedHeight;
        if (boxWidth / boxHeight > ratio)
        {
            renderedHeight = boxHeight;
            renderedWidth = boxHeight * ratio;
        }
        else
        {
            renderedWidth = boxWidth;
            renderedHeight = boxWidth / ratio;
        }

        var x = ProcedureLayout.LogoBoxX1 + (boxWidth - renderedWidth) / 2.0;
        var y = ProcedureLayout.LogoBoxY1 + (boxHeight - renderedHeight) / 2.0;

        var result = container
            .PaddingTop((float)y)
            .PaddingLeft((float)x)
            .Width((float)renderedWidth)
            .Height((float)renderedHeight);

        result.Image(logo);
        return result;
    }

    private static (int Width, int Height) PngSize(byte[] png)
    {
        if (png.Length < 24)
        {
            throw new InvalidOperationException("Invalid PNG data.");
        }

        var width = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4));
        var height = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4));
        return (width, height);
    }

    private static byte[] LoadLogo()
    {
        var assembly = typeof(OperationalProcedureRenderer).Assembly;
        using var stream = assembly.GetManifestResourceStream(LogoResourceName)
            ?? throw new InvalidOperationException($"Embedded logo '{LogoResourceName}' was not found.");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private static double Ascent(double sizePt, bool bold, string family) =>
        family == PdfFontRegistry.SansFamilyName
            ? ProcedureLayout.SansAscentRatio * sizePt
            : (bold ? SummarySheetLayout.BoldAscentRatio : SummarySheetLayout.RegularAscentRatio) * sizePt;
}

public enum ProcedureAlign
{
    Left,
    Center,
    Right
}

public sealed record ProcedureRun(
    string Text,
    double X,
    double Baseline,
    double Size,
    bool Bold,
    string Family,
    ProcedureAlign Align,
    string? Color)
{
    public ProcedureRun(string text, double x, double baseline, double size, bool bold, string family)
        : this(text, x, baseline, size, bold, family, ProcedureAlign.Left, null)
    {
    }

    public ProcedureRun(string text, double x, double baseline, double size, bool bold, string family, ProcedureAlign align)
        : this(text, x, baseline, size, bold, family, align, null)
    {
    }
}

public sealed record ProcedureRect(
    double X1,
    double X2,
    double Y1,
    double Y2,
    string Color);

internal sealed record ItemWord(string Text, bool Bold, double Width);

internal sealed class ParsedProcedureDocument
{
    public string OperationName { get; set; } = "-";
    public string ArtNumber { get; set; } = "-";
    public string PlanNumber { get; set; } = "-";
    public List<ParsedProcedurePage> Pages { get; set; } = [];
}

internal sealed class ParsedProcedurePage
{
    public int Number { get; set; }
    public List<ParsedProcedureSection> Sections { get; set; } = [];
}

internal sealed class ParsedProcedureSection
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = [];
    public List<string> References { get; set; } = [];
}

internal static class ProcedureContentParser
{
    public static ParsedProcedureDocument Parse(string content)
    {
        var doc = new ParsedProcedureDocument();
        using var reader = new StringReader(content);
        string? line;
        ParsedProcedurePage? currentPage = null;
        ParsedProcedureSection? currentSection = null;
        string currentList = string.Empty;

        while ((line = reader.ReadLine()) != null)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            if (trimmed == "Guindastes Ribas")
            {
                currentPage = new ParsedProcedurePage { Number = doc.Pages.Count + 1 };
                doc.Pages.Add(currentPage);
                currentSection = null;
                currentList = string.Empty;
                continue;
            }

            if (trimmed.StartsWith("Operação:", StringComparison.OrdinalIgnoreCase))
            {
                var val = trimmed["Operação:".Length..].Trim();
                doc.OperationName = string.IsNullOrEmpty(val) ? "-" : val;
                continue;
            }
            if (trimmed.StartsWith("ART:", StringComparison.OrdinalIgnoreCase))
            {
                var val = trimmed["ART:".Length..].Trim();
                doc.ArtNumber = string.IsNullOrEmpty(val) ? "-" : val;
                continue;
            }
            if (trimmed.StartsWith("RIGGING:", StringComparison.OrdinalIgnoreCase))
            {
                var val = trimmed["RIGGING:".Length..].Trim();
                doc.PlanNumber = string.IsNullOrEmpty(val) ? "-" : val;
                continue;
            }

            var sectionMatch = Regex.Match(trimmed, @"^([1-5])\.\s*(.*)");
            if (sectionMatch.Success)
            {
                currentSection = new ParsedProcedureSection
                {
                    Number = int.Parse(sectionMatch.Groups[1].Value),
                    Title = sectionMatch.Groups[2].Value.Trim()
                };
                currentPage?.Sections.Add(currentSection);
                currentList = string.Empty;
                continue;
            }

            if (trimmed.StartsWith("Objetivo:", StringComparison.OrdinalIgnoreCase))
            {
                if (currentSection != null)
                {
                    currentSection.Objective = trimmed["Objetivo:".Length..].Trim();
                }
                currentList = string.Empty;
                continue;
            }

            if (trimmed.StartsWith("Passos:", StringComparison.OrdinalIgnoreCase))
            {
                currentList = "steps";
                continue;
            }

            if (trimmed.StartsWith("Referências:", StringComparison.OrdinalIgnoreCase))
            {
                currentList = "refs";
                continue;
            }

            if (trimmed.StartsWith("- ") && currentSection != null)
            {
                var item = trimmed[2..].Trim();
                if (currentList == "steps") currentSection.Steps.Add(item);
                else if (currentList == "refs") currentSection.References.Add(item);
                continue;
            }
        }

        return doc;
    }
}
