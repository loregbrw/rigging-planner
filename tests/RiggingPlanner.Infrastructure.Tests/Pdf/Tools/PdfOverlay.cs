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

        if (reference.Width != generated.Width || reference.Height != generated.Height)
        {
            throw new InvalidOperationException(
                $"Overlay images must have the same size. Reference: {reference.Width}x{reference.Height}, generated: {generated.Width}x{generated.Height}.");
        }

        var referencePixels = reference.Pixels;
        var generatedPixels = generated.Pixels;

        int totalPixels = referencePixels.Length;
        int divergent = 0;
        var output = new SKColor[totalPixels];

        for (var i = 0; i < totalPixels; i++)
        {
            var referenceInk = IsInk(referencePixels[i], inkLuminanceThreshold);
            var generatedInk = IsInk(generatedPixels[i], inkLuminanceThreshold);

            if (referenceInk && generatedInk)
            {
                output[i] = new SKColor(60, 60, 60);
            }
            else if (referenceInk)
            {
                output[i] = SKColors.Red;
                divergent++;
            }
            else if (generatedInk)
            {
                output[i] = SKColors.Cyan;
                divergent++;
            }
            else
            {
                output[i] = SKColors.White;
            }
        }

        var bitmap = new SKBitmap(reference.Width, reference.Height);
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
