namespace RiggingPlanner.Domain.Entities;

public sealed class PlanDrawing
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public Guid FileId { get; set; }
    public int SortOrder { get; set; }
    public RiggingPlan Plan { get; set; } = null!;
    public StoredFile File { get; set; } = null!;
}
