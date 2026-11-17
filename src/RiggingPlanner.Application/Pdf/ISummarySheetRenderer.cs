using RiggingPlanner.Domain;

namespace RiggingPlanner.Application.Pdf;

/// <summary>
/// Renders the summary sheet (page 1, "Capacidade de carga") as a standalone PDF.
/// Each stage renderer returns a single PDF; the assembler (stage 6) joins them.
/// </summary>
public interface ISummarySheetRenderer
{
    byte[] Render(SummarySheetModel model);
}
