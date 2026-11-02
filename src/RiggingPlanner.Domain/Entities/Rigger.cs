namespace RiggingPlanner.Domain.Entities;

public sealed class Rigger
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string RppNumber { get; set; } = string.Empty;
    public string? CreaNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<RiggingPlan> Plans { get; } = [];
}
