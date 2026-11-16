using PDFtoImage;
using SkiaSharp;

namespace RiggingPlanner.Infrastructure.Tests.Pdf.Tools;

/// <summary>
/// Rasterizes a PDF page to PNG using PDFium (via PDFtoImage).
/// </summary>
public static class PdfRasterizer
{
    public const int DefaultDpi = 150;

    public static byte[] RasterizeToPng(byte[] pdfBytes, int page = 1, int dpi = DefaultDpi)
    {
        using var bitmap = Conversion.ToImage(pdfBytes, page, options: new RenderOptions { Dpi = dpi });
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    public static SKBitmap Decode(byte[] pngBytes) => SKBitmap.Decode(pngBytes);
}
