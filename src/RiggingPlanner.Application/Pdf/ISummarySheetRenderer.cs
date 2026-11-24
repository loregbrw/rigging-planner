using RiggingPlanner.Domain;

namespace RiggingPlanner.Application.Pdf;

/// <summary>
/// Renders the summary sheet (page 1, "Capacidade de carga") as a standalone PDF.
/// Renders a single PDF document to be joined by the full-plan assembler.
/// </summary>
public interface ISummarySheetRenderer
{
    byte[] Render(SummarySheetModel model);
}
