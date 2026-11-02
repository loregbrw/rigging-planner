namespace RiggingPlanner.Domain.Entities;

public sealed class AccessoryEvent
{
    public Guid Id { get; set; }
    public Guid AccessoryId { get; set; }
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Accessory Accessory { get; set; } = null!;
}
