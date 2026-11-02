namespace RiggingPlanner.Domain.Entities;

public sealed class Accessory
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? LengthM { get; set; }
    public decimal? CapacityT { get; set; }
    public decimal UnitWeightKg { get; set; }
    public string? DrawingRef { get; set; }
    public string Status { get; set; } = "active";
    public DateTime? ReviewDueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<AccessoryEvent> Events { get; } = [];
    public ICollection<PlanAccessory> PlanItems { get; } = [];
}
