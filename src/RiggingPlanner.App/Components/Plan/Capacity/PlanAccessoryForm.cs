using System.ComponentModel.DataAnnotations;

namespace RiggingPlanner.App.Components.Plan.Capacity;

public sealed class PlanAccessoryForm
{
    public Guid? EditingId { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? DrawingRef { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Quantity { get; set; } = 1;

    [Range(0.01, double.MaxValue)]
    public decimal? UnitWeightKg { get; set; }
}
