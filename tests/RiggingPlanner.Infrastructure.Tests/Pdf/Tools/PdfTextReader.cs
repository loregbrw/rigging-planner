using System.Text.Json;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace RiggingPlanner.Infrastructure.Tests.Pdf.Tools;

public sealed record PdfWordInfo(
    string Text,
    double X0,
    double X1,
    double Top,
    double Bottom,
    string FontName,
    double Size);

public sealed record PdfPageWords(
    int PageNumber,
    double Width,
    double Height,
    IReadOnlyList<PdfWordInfo> Words);

/// <summary>
/// Extracts words with coordinates in points, origin at the top-left corner,
/// matching the coordinate system used by the reference JSON files.
/// Words keep their natural extraction order.
/// </summary>
public static class PdfTextReader
{
    public static IReadOnlyList<PdfPageWords> ReadPages(string pdfPath) =>
        ReadPages(File.ReadAllBytes(pdfPath));

    public static IReadOnlyList<PdfPageWords> ReadPages(byte[] pdfBytes)
    {
        using var document = PdfDocument.Open(pdfBytes);
        var pages = new List<PdfPageWords>();

        foreach (var page in document.GetPages())
        {
            var pageHeight = (double)page.Height;
            // Default extractor: its word segmentation matches the reference JSON
            // (NearestNeighbourWordExtractor over-segments this document: 450 words vs 295).
            var words = page.GetWords()
                .Select(w => ToWordInfo(w, pageHeight))
                .ToList();

            pages.Add(new PdfPageWords(page.Number, (double)page.Width, pageHeight, words));
        }

        return pages;
    }

    private static PdfWordInfo ToWordInfo(Word word, double pageHeight)
    {
        var boundingBox = word.BoundingBox;
        var top = pageHeight - (double)boundingBox.TopRight.Y;
        var bottom = pageHeight - (double)boundingBox.BottomLeft.Y;
        return new PdfWordInfo(
            word.Text,
            (double)boundingBox.BottomLeft.X,
            (double)boundingBox.TopRight.X,
            top,
            bottom,
            word.FontName ?? string.Empty,
            bottom - top);
    }
}

/// <summary>
/// Loads a reference JSON with measured words/rects from the example plan PDF
/// (coordinates in pt, top-left origin).
/// </summary>
public sealed class ReferenceSheet
{
    public sealed record ReferenceRect(
        double X0,
        double X1,
        double Top,
        double Bottom,
        string Kind);

    public double Width { get; private init; }
    public double Height { get; private init; }
    public IReadOnlyList<PdfWordInfo> Words { get; private init; } = [];
    public IReadOnlyList<ReferenceRect> Rects { get; private init; } = [];

    public static ReferenceSheet Load(string jsonPath)
    {
        var json = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(jsonPath));
        return FromJson(json);
    }

    public static ReferenceSheet FromJson(JsonElement json)
    {
        var page = json.GetProperty("page");
        var words = json.GetProperty("words").EnumerateArray()
            .Select(w => new PdfWordInfo(
                w.GetProperty("text").GetString() ?? string.Empty,
                w.GetProperty("x0").GetDouble(),
                w.GetProperty("x1").GetDouble(),
                w.GetProperty("top").GetDouble(),
                w.GetProperty("bottom").GetDouble(),
                w.GetProperty("font").GetString() ?? string.Empty,
                w.GetProperty("size").GetDouble()))
            .ToList();

        var rects = json.TryGetProperty("rects", out var rectsElement)
            ? rectsElement.EnumerateArray()
                .Select(r => new ReferenceRect(
                    r.GetProperty("x0").GetDouble(),
                    r.GetProperty("x1").GetDouble(),
                    r.GetProperty("top").GetDouble(),
                    r.GetProperty("bottom").GetDouble(),
                    r.TryGetProperty("kind", out var kind) ? kind.GetString() ?? string.Empty : string.Empty))
                .ToList()
            : new List<ReferenceRect>();

        return new ReferenceSheet
        {
            Width = page.GetProperty("width").GetDouble(),
            Height = page.GetProperty("height").GetDouble(),
            Words = words,
            Rects = rects
        };
    }
}
