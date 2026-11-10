namespace RiggingPlanner.Domain.Entities;

public sealed class PlanAccessory
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public string Section { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DrawingRef { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitWeightKg { get; set; }
    public int SortOrder { get; set; }
    public RiggingPlan Plan { get; set; } = null!;
}
