namespace RiggingPlanner.Domain.Entities;

public sealed class Accessory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitWeightKg { get; set; }
    public Guid? TagId { get; set; }
    public string Status { get; set; } = "active";
    public DateTime? ReviewDueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<AccessoryEvent> Events { get; } = [];
    public ICollection<PlanAccessory> PlanItems { get; } = [];
    public Tag? Tag { get; set; }
}
