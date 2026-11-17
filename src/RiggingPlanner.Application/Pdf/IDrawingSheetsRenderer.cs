using RiggingPlanner.Domain;

namespace RiggingPlanner.Application.Pdf;

/// <summary>
/// Renders the drawing sheets (frame and stamp) and image layouts.
/// </summary>
public interface IDrawingSheetsRenderer
{
    byte[] RenderFrameOnly(DrawingSheetStampModel model);

    byte[] RenderWithImages(DrawingSheetStampModel model, IReadOnlyList<DrawingSheetImageItem> images);
}

public sealed record DrawingSheetImageItem(
    Guid Id,
    byte[] Data,
    int WidthPx,
    int HeightPx);
