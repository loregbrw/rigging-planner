namespace RiggingPlanner.Domain.Entities;

public sealed class StoredFile
{
    public Guid Id { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<EquipmentReport> EquipmentReports { get; } = [];
    public ICollection<PlanDrawing> PlanDrawings { get; } = [];
    public ICollection<RiggingPlan> PlanArtFiles { get; } = [];
}
