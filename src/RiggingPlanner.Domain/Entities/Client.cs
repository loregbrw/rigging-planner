namespace RiggingPlanner.Domain.Entities;

public sealed class Client
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<RiggingPlan> Plans { get; } = [];
}
