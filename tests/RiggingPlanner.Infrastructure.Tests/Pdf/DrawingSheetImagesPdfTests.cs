using SkiaSharp;
using UglyToad.PdfPig;
using RiggingPlanner.Application.Pdf;
using RiggingPlanner.Domain;
using RiggingPlanner.Infrastructure.Pdf;
using RiggingPlanner.Infrastructure.Tests.Fixtures;
using RiggingPlanner.Infrastructure.Tests.Pdf.Tools;
using Xunit;
using Xunit.Abstractions;

namespace RiggingPlanner.Infrastructure.Tests.Pdf;

public sealed class DrawingSheetImagesPdfTests(ITestOutputHelper output)
{
    private const string Stage = "stage-2b";

    [Fact]
    public void RenderWithImages_SyntheticImages_ProducesCorrectPagesAndStamp()
    {
        var (plan, accessories) = TestPlans.ReferencePlan();
        var baseModel = DrawingSheetStampBuilder.Build(plan, accessories, 1, 1);

        var imageItems = new List<DrawingSheetImageItem>
        {
            new(Guid.NewGuid(), CreateSyntheticImage(1200, 800, SKColors.Blue, "Drawing 1 Wide"), 1200, 800),
            new(Guid.NewGuid(), CreateSyntheticImage(800, 1200, SKColors.Red, "Drawing 2 Tall"), 800, 1200),
            new(Guid.NewGuid(), CreateSyntheticImage(1000, 1000, SKColors.Green, "Drawing 3 Square"), 1000, 1000),
            new(Guid.NewGuid(), CreateSyntheticImage(1400, 900, SKColors.Orange, "Drawing 4 Wide"), 1400, 900),
            new(Guid.NewGuid(), CreateSyntheticImage(900, 1400, SKColors.Purple, "Drawing 5 Tall"), 900, 1400),
        };

        var renderer = new DrawingSheetsRenderer();
        var pdfBytes = renderer.RenderWithImages(baseModel, imageItems);

        ReviewOutput.WriteFile(Stage, "synthetic-drawings.pdf", pdfBytes);

        using var document = PdfDocument.Open(pdfBytes);
        var pageCount = document.NumberOfPages;
        output.WriteLine($"Generated {pageCount} pages for 5 synthetic images.");
        Assert.True(pageCount > 0);

        for (var i = 0; i < pageCount; i++)
        {
            var png = PdfRasterizer.RasterizeToPng(pdfBytes, page: i);
            ReviewOutput.WriteFile(Stage, $"synthetic-sheet-{i + 1}.png", png);
        }

        for (var i = 0; i < pageCount; i++)
        {
            var page = document.GetPage(i + 1);
            var text = page.Text;
            Assert.Contains($"FOLHA {i + 1} DE {pageCount}", text);
        }
    }

    [Fact]
    public void RenderWithImages_RealCaptures_IfAvailable()
    {
        var fixturesDir = Path.Combine(AppContext.BaseDirectory, "Fixtures", "drawings");
        if (!Directory.Exists(fixturesDir) || Directory.GetFiles(fixturesDir).Length == 0)
        {
            output.WriteLine("Skipping real captures test: Fixtures/drawings/ is empty or missing.");
            return;
        }

        var drawingFiles = Directory.GetFiles(fixturesDir)
            .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f)
            .ToList();

        if (drawingFiles.Count == 0)
        {
            output.WriteLine("Skipping real captures test: no image files found in Fixtures/drawings/.");
            return;
        }

        var imageItems = new List<DrawingSheetImageItem>();
        foreach (var file in drawingFiles)
        {
            var bytes = File.ReadAllBytes(file);
            var info = SKBitmap.DecodeBounds(bytes);
            if (info.Width == 0) info = new SKImageInfo(800, 600);
            imageItems.Add(new DrawingSheetImageItem(Guid.NewGuid(), bytes, info.Width, info.Height));
        }

        var (plan, accessories) = TestPlans.ReferencePlan();
        var baseModel = DrawingSheetStampBuilder.Build(plan, accessories, 1, 1);

        var renderer = new DrawingSheetsRenderer();
        var pdfBytes = renderer.RenderWithImages(baseModel, imageItems);

        ReviewOutput.WriteFile(Stage, "real-drawings.pdf", pdfBytes);

        using var document = PdfDocument.Open(pdfBytes);
        var pageCount = document.NumberOfPages;
        output.WriteLine($"Generated {pageCount} pages for {imageItems.Count} real drawings.");

        for (var i = 0; i < pageCount; i++)
        {
            var png = PdfRasterizer.RasterizeToPng(pdfBytes, page: i);
            ReviewOutput.WriteFile(Stage, $"real-sheet-{i + 1}.png", png);
        }

        Assert.True(pageCount > 0);
    }

    private static byte[] CreateSyntheticImage(int widthPx, int heightPx, SKColor color, string text)
    {
        using var bitmap = new SKBitmap(widthPx, heightPx);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
        using var paint = new SKPaint
        {
            Color = color,
            Style = SKPaintStyle.Fill,
        };
        canvas.DrawRect(10, 10, widthPx - 20, heightPx - 20, paint);

        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
        };
        using var font = new SKFont(SKTypeface.Default, Math.Max(16, widthPx / 20f));
        canvas.DrawText(text, 30, heightPx / 2f, SKTextAlign.Left, font, textPaint);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
