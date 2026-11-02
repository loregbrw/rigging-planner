namespace RiggingPlanner.Domain.Entities;

public sealed class PlanNumberCounter
{
    public string YearMonth { get; set; } = string.Empty;
    public int LastSequence { get; set; }
}
