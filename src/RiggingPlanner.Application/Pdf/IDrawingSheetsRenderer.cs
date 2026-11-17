using RiggingPlanner.Domain;

namespace RiggingPlanner.Application.Pdf;

/// <summary>
/// Renders the drawing sheets (frame and stamp). Stage 2A renders a single sheet
/// without images; the image layout arrives in stage 2B.
/// </summary>
public interface IDrawingSheetsRenderer
{
    byte[] RenderFrameOnly(DrawingSheetStampModel model);
}
