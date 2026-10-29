namespace RiggingPlanner.Domain.Entities;

public sealed class EquipmentReport
{
    public Guid Id { get; set; }
    public Guid EquipmentId { get; set; }
    public Guid FileId { get; set; }
    public string ArtNumber { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public Equipment Equipment { get; set; } = null!;
    public StoredFile File { get; set; } = null!;
    public ICollection<RiggingPlan> Plans { get; } = [];
}
