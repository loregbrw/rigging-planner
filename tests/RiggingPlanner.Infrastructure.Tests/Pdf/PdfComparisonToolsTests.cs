using RiggingPlanner.Infrastructure.Tests.Pdf.Tools;
using Xunit;

namespace RiggingPlanner.Infrastructure.Tests.Pdf;

public class PdfComparisonToolsTests
{
    private static string FixturePath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);

    private static byte[] ReadFixture(string fileName) =>
        File.ReadAllBytes(FixturePath(fileName));

    // The reference JSON was measured with a different tool: word boxes differ (~1 pt) and a few
    // words are segmented differently ("Rigger"+"Giuliano" vs "RiggerGiuliano", "-"+"RPP-12345"
    // vs "-RPP-12345", "2607001" vs "260703"+"6"). Stage-0 sanity therefore compares text
    // content only (decision recorded in docs/pdf-export.md, section 5):
    // positions are validated visually against the overlay from stage 1 onwards.
    [Fact]
    public void PdfTextReader_ExamplePdf_TextContentMatchesReferenceJson()
    {
        var pdfBytes = ReadFixture("reference-plan.pdf");
        var reference = ReferenceSheet.Load(FixturePath("summary-sheet.reference.json"));

        var pages = PdfTextReader.ReadPages(pdfBytes);
        var page = Assert.Single(pages, p => p.PageNumber == 1);

        Assert.Equal(reference.Width, page.Width, precision: 1);
        Assert.Equal(reference.Height, page.Height, precision: 1);

        Assert.NotEmpty(reference.Words);
        Assert.NotEmpty(page.Words);

        // The PDF extraction contains a few whitespace-only tokens; content comparison
        // ignores them and all whitespace characters.
        static string Content(IEnumerable<PdfWordInfo> words) =>
            string.Concat(words
                .Where(w => !string.IsNullOrWhiteSpace(w.Text))
                .SelectMany(w => w.Text)
                .Where(c => !char.IsWhiteSpace(c)));

        // a) No text lost, added or changed: identical character sequence (order-insensitive).
        var pdfChars = Content(page.Words).OrderBy(c => c).ToArray();
        var referenceChars = Content(reference.Words).OrderBy(c => c).ToArray();
        Assert.Equal(referenceChars, pdfChars);

        // b) Every reference word is a contiguous run in the extraction-order PDF text.
        var pdfReadingOrder = string.Concat(
            page.Words
                .Where(w => !string.IsNullOrWhiteSpace(w.Text))
                .Select(w => w.Text));
        foreach (var word in reference.Words.Where(w => !string.IsNullOrWhiteSpace(w.Text)))
        {
            Assert.Contains(word.Text, pdfReadingOrder, StringComparison.Ordinal);
        }

        // c) Symmetric: every extracted word is a contiguous run in the reference text
        // (natural JSON array order).
        var referenceReadingOrder = string.Concat(
            reference.Words
                .Where(w => !string.IsNullOrWhiteSpace(w.Text))
                .Select(w => w.Text));
        foreach (var word in page.Words.Where(w => !string.IsNullOrWhiteSpace(w.Text)))
        {
            Assert.Contains(word.Text, referenceReadingOrder, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PdfOverlay_ExampleAgainstItself_HasZeroDivergence()
    {
        var pdfBytes = ReadFixture("reference-plan.pdf");

        var referencePng = PdfRasterizer.RasterizeToPng(pdfBytes, page: 0);
        var generatedPng = PdfRasterizer.RasterizeToPng(pdfBytes, page: 0);

        var result = PdfOverlay.Compose(referencePng, generatedPng);

        Assert.Equal(0, result.DivergentPixels);
        Assert.Equal(0.0, result.DivergencePercent);

        var outputPath = ReviewOutput.WriteFile(0, "self-overlay-reference-plan-2607001.png", result.Png);
        ReviewOutput.WriteText(0, "diff-metrics.txt",
            $"overlay: reference-plan.pdf page 1 vs itself\n" +
            $"divergent pixels: {result.DivergentPixels}/{result.TotalPixels} ({result.DivergencePercent:F4}%)\n" +
            $"output: {outputPath}\n");

        Assert.True(File.Exists(outputPath));
    }
}
