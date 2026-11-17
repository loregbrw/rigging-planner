using SkiaSharp;

namespace RiggingPlanner.Infrastructure.Tests.Pdf.Tools;

/// <summary>
/// Composes a visual overlay of two rasterized pages: reference in red,
/// generated in cyan, coincident ink in dark gray, background white.
/// </summary>
public static class PdfOverlay
{
    public sealed record OverlayResult(byte[] Png, int DivergentPixels, int TotalPixels, double DivergencePercent);

    public static OverlayResult Compose(byte[] referencePng, byte[] generatedPng, byte inkLuminanceThreshold = 200)
    {
        using var reference = SKBitmap.Decode(referencePng);
        using var generated = SKBitmap.Decode(generatedPng);

        // Rasterization rounds the page size to whole pixels, so the two pages may
        // differ by 1 px; the overlay compares the common area.
        var width = Math.Min(reference.Width, generated.Width);
        var height = Math.Min(reference.Height, generated.Height);

        var referencePixels = reference.Pixels;
        var generatedPixels = generated.Pixels;

        int totalPixels = width * height;
        int divergent = 0;
        var output = new SKColor[totalPixels];

        for (var y = 0; y < height; y++)
        {
            var referenceRow = y * reference.Width;
            var generatedRow = y * generated.Width;
            var outputRow = y * width;
            for (var x = 0; x < width; x++)
            {
                var referenceInk = IsInk(referencePixels[referenceRow + x], inkLuminanceThreshold);
                var generatedInk = IsInk(generatedPixels[generatedRow + x], inkLuminanceThreshold);
                var index = outputRow + x;

                if (referenceInk && generatedInk)
                {
                    output[index] = new SKColor(60, 60, 60);
                }
                else if (referenceInk)
                {
                    output[index] = SKColors.Red;
                    divergent++;
                }
                else if (generatedInk)
                {
                    output[index] = SKColors.Cyan;
                    divergent++;
                }
                else
                {
                    output[index] = SKColors.White;
                }
            }
        }

        var bitmap = new SKBitmap(width, height);
        bitmap.Pixels = output;

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var png = data.ToArray();
        bitmap.Dispose();

        var percent = totalPixels == 0 ? 0 : divergent * 100.0 / totalPixels;
        return new OverlayResult(png, divergent, totalPixels, percent);
    }

    private static bool IsInk(SKColor pixel, byte threshold) =>
        Math.Min(pixel.Red, Math.Min(pixel.Green, pixel.Blue)) < threshold;
}
