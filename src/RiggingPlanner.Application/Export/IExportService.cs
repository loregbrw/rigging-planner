namespace RiggingPlanner.Application.Export;

public sealed record PlanExportDocument(byte[] Pdf, string FileName, IReadOnlyList<string> Warnings);

public interface IExportService
{
    Task<PlanExportDocument> BuildAsync(Guid planId);
    Task<IReadOnlyList<string>> GetWarningsAsync(Guid planId);
}
