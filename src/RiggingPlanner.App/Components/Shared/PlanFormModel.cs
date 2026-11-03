using System.ComponentModel.DataAnnotations;

namespace RiggingPlanner.App.Components.Shared;

public sealed class PlanFormModel
{
    [Required(ErrorMessage = "Informe o nome da operação.")]
    public string OperationName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o local.")]
    public string Location { get; set; } = string.Empty;

    public DateTime PlanDate { get; set; } = DateTime.Today;
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string ClientCnpj { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a descrição da carga.")]
    public string LoadDescription { get; set; } = string.Empty;

    public decimal? WeightKg { get; set; }
    public decimal? LengthM { get; set; }
    public decimal? WidthM { get; set; }
    public string? Observations { get; set; }
    public Guid? EquipmentId { get; set; }
    public Guid? RiggerId { get; set; }
}
