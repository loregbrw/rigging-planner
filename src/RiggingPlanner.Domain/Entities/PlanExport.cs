namespace RiggingPlanner.Domain.Entities;

public sealed class PlanExport
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public DateTime ExportedAt { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileSha256 { get; set; } = string.Empty;
    public bool WasFinal { get; set; }
    public RiggingPlan Plan { get; set; } = null!;
}
