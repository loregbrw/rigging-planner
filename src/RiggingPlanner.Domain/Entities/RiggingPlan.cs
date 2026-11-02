namespace RiggingPlanner.Domain.Entities;

public sealed class RiggingPlan
{
    public Guid Id { get; set; }
    public string PlanNumber { get; set; } = string.Empty;
    public Guid? FolderId { get; set; }
    public string Status { get; set; } = "draft";
    public DateTime? FinalizedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime PlanDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientCnpj { get; set; }
    public string LoadDescription { get; set; } = string.Empty;
    public decimal? LengthM { get; set; }
    public decimal? WidthM { get; set; }
    public decimal? HeightM { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? ExtraLoadKg { get; set; }
    public Guid? EquipmentId { get; set; }
    public string? TruckModel { get; set; }
    public string? LicensePlate { get; set; }
    public string? ChassisNumber { get; set; }
    public int? ManufactureYear { get; set; }
    public string? CraneBrand { get; set; }
    public string? CraneModel { get; set; }
    public decimal? CraneNominalCapacity { get; set; }
    public decimal? BoomLengthM { get; set; }
    public decimal? OperatingRadiusM { get; set; }
    public decimal? LoadCapacityT { get; set; }
    public string? QuadrantStart { get; set; }
    public string? QuadrantEnd { get; set; }
    public decimal? FlatStrapUtilizationPct { get; set; }
    public decimal? TubularStrapUtilizationPct { get; set; }
    public decimal? WireRopeSlingUtilizationPct { get; set; }
    public decimal? ShackleUtilizationPct { get; set; }
    public decimal? FootingForceKgf { get; set; }
    public decimal? MinGroundResistanceKgfCm2 { get; set; }
    public decimal? MaxWindSpeedMs { get; set; }
    public Guid? RiggerId { get; set; }
    public string? RiggerName { get; set; }
    public string? RiggerRpp { get; set; }
    public string? RiggerCrea { get; set; }
    public string? SupervisorName { get; set; }
    public string? ArtNumber { get; set; }
    public Guid? ArtFileId { get; set; }
    public Guid? EquipmentReportId { get; set; }
    public string? Observations { get; set; }
    public Folder? Folder { get; set; }
    public Client? Client { get; set; }
    public Equipment? Equipment { get; set; }
    public Rigger? Rigger { get; set; }
    public StoredFile? ArtFile { get; set; }
    public EquipmentReport? EquipmentReport { get; set; }
    public ICollection<PlanAccessory> Accessories { get; } = [];
    public ICollection<PlanDrawing> Drawings { get; } = [];
    public ICollection<PlanExport> Exports { get; } = [];
}
