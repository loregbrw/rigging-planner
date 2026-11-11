namespace RiggingPlanner.Domain.Entities;

public sealed class Equipment
{
    public Guid Id { get; set; }
    public string EquipmentType { get; set; } = string.Empty;
    public string TruckModel { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string ChassisNumber { get; set; } = string.Empty;
    public int? ManufactureYear { get; set; }
    public string CraneBrand { get; set; } = string.Empty;
    public string CraneModel { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<EquipmentReport> Reports { get; } = [];
    public ICollection<RiggingPlan> Plans { get; } = [];
}
