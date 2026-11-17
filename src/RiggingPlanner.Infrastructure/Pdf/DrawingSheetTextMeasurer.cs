using SkiaSharp;

namespace RiggingPlanner.Infrastructure.Pdf;

/// <summary>
/// Measures text widths and ink boxes (points) for the drawing-sheet fonts, using the
/// embedded TTFs and the same shaping rules as the renderer (no kerning, no ligatures).
/// The renderer centers glyph ink inside the reference cells, so the ink box - not the
/// advance box - is what matters here.
/// </summary>
public static class DrawingSheetTextMeasurer
{
    // Baseline offset inside the text container, calibrated against the real QuestPDF
    // pipeline (QuestPDF lays the line out using the font ascent; it does not add leading).
    private const double GothicAscentRatio = 0.82;
    private const double BrandAscentRatio = 1.06884766;

    private static readonly object Lock = new();
    private static readonly Dictionary<string, (SKTypeface Typeface, SKData Data)> Typefaces = new();

    public static double AscentRatio(string family) =>
        family == PdfFontRegistry.BrandFamilyName ? BrandAscentRatio : GothicAscentRatio;

    public static SKRect InkBounds(string text, double sizePt, string family)
    {
        lock (Lock)
        {
            using var font = new SKFont(GetTypeface(family), (float)sizePt);
            font.MeasureText(text, out var bounds);
            return bounds;
        }
    }

    public static double AdvanceWidth(string text, double sizePt, string family)
    {
        lock (Lock)
        {
            using var font = new SKFont(GetTypeface(family), (float)sizePt);
            return font.MeasureText(text);
        }
    }

    /// <summary>
    /// Greedy word wrapping with the measured advances. A single word wider than the
    /// available width is kept whole: nothing is ever truncated.
    /// </summary>
    public static IReadOnlyList<string> WrapLines(string text, double sizePt, string family, double maxWidth)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return [text];
        }

        var lines = new List<string>();
        var current = words[0];
        for (var i = 1; i < words.Length; i++)
        {
            var candidate = current + " " + words[i];
            if (AdvanceWidth(candidate, sizePt, family) <= maxWidth)
            {
                current = candidate;
            }
            else
            {
                lines.Add(current);
                current = words[i];
            }
        }

        lines.Add(current);
        return lines;
    }

    private static SKTypeface GetTypeface(string family)
    {
        if (Typefaces.TryGetValue(family, out var cached))
        {
            return cached.Typeface;
        }

        var fileName = family == PdfFontRegistry.BrandFamilyName
            ? PdfFontRegistry.BrandFontFileName
            : PdfFontRegistry.GothicFontFileName;

        var assembly = typeof(DrawingSheetTextMeasurer).Assembly;
        var resourceName = $"RiggingPlanner.Infrastructure.Pdf.Fonts.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded font resource '{resourceName}' was not found.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        var data = SKData.CreateCopy(memory.ToArray());
        var typeface = SKTypeface.FromData(data)
            ?? throw new InvalidOperationException($"Embedded font '{fileName}' could not be loaded.");

        Typefaces[family] = (typeface, data);
        return typeface;
    }
}
