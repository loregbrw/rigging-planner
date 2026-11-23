using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Application.Pdf;

/// <summary>
/// Renders the operational procedure (2 pages A4) as a standalone PDF using QuestPDF.
/// </summary>
public interface IOperationalProcedureRenderer
{
    byte[] Render(RiggingPlan plan, PlanProcedureSnapshot? snapshot);

    byte[] RenderFromContent(string content);
}
