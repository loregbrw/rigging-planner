namespace RiggingPlanner.Application.Pdf;

/// <summary>
/// Assembles the complete plan PDF in the fixed order: summary sheet, drawing sheets,
/// plan ART, operational procedure, equipment report. Optional parts are skipped when absent.
/// </summary>
public sealed class FullPlanPdfAssembler(IPdfDocumentMerger merger)
{
    public byte[] Assemble(byte[] summarySheet, byte[]? drawingSheets, byte[]? planArt, byte[] procedure, byte[]? equipmentReport)
    {
        ArgumentNullException.ThrowIfNull(summarySheet);
        ArgumentNullException.ThrowIfNull(procedure);

        var documents = new List<byte[]> { summarySheet };
        if (drawingSheets is not null)
        {
            documents.Add(drawingSheets);
        }
        if (planArt is not null)
        {
            documents.Add(planArt);
        }
        documents.Add(procedure);
        if (equipmentReport is not null)
        {
            documents.Add(equipmentReport);
        }

        return merger.Merge(documents);
    }
}
