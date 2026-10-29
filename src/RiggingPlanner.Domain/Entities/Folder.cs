namespace RiggingPlanner.Domain.Entities;

public sealed class Folder
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<RiggingPlan> Plans { get; } = [];
}
