namespace RiggingPlanner.Domain.Entities;

public sealed class PlanProcedureSnapshot
{
    public Guid PlanId { get; set; }
    public string CraneModelCode { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ContentSha256 { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public RiggingPlan? Plan { get; set; }
}
