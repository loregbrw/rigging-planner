using System.ComponentModel.DataAnnotations;

namespace RiggingPlanner.App.Components.Shared;

public sealed class PlanFormModel : IValidatableObject
{
    [Required(ErrorMessage = "Informe a descrição da operação.")]
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
    public decimal? HeightM { get; set; }
    public string? Observations { get; set; }
    public Guid? EquipmentId { get; set; }
    public string EquipmentModel { get; set; } = string.Empty;
    public string EquipmentTruckModel { get; set; } = string.Empty;
    public string EquipmentChassisNumber { get; set; } = string.Empty;
    public string EquipmentLicensePlate { get; set; } = string.Empty;
    public int? EquipmentManufactureYear { get; set; }
    public bool IsNewEquipment { get; set; }
    public Guid? RiggerId { get; set; }
    public string RiggerFullName { get; set; } = string.Empty;
    public string RiggerDisplayName { get; set; } = string.Empty;
    public string RiggerRppNumber { get; set; } = string.Empty;
    public string RiggerCreaNumber { get; set; } = string.Empty;
    public bool IsNewRigger { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PlanDate == default)
        {
            yield return new ValidationResult("Informe a data do plano.", [nameof(PlanDate)]);
        }

        if (string.IsNullOrWhiteSpace(ClientName))
        {
            yield return new ValidationResult("Selecione ou informe um cliente.", [nameof(ClientName)]);
        }
        else if (ClientId is null && !IsValidCnpj(ClientCnpj))
        {
            yield return new ValidationResult(
                "Informe um CNPJ válido para o novo cliente.",
                [nameof(ClientCnpj)]);
        }

        if (string.IsNullOrWhiteSpace(EquipmentModel))
        {
            yield return new ValidationResult("Selecione ou informe um equipamento.", [nameof(EquipmentModel)]);
        }
        else if (EquipmentId is null)
        {
            if (string.IsNullOrWhiteSpace(EquipmentTruckModel))
            {
                yield return new ValidationResult("Informe o caminhão do novo equipamento.", [nameof(EquipmentTruckModel)]);
            }
            if (string.IsNullOrWhiteSpace(EquipmentChassisNumber))
            {
                yield return new ValidationResult("Informe o chassi do novo equipamento.", [nameof(EquipmentChassisNumber)]);
            }
            if (string.IsNullOrWhiteSpace(EquipmentLicensePlate))
            {
                yield return new ValidationResult("Informe a placa do novo equipamento.", [nameof(EquipmentLicensePlate)]);
            }
        }

        if (string.IsNullOrWhiteSpace(RiggerDisplayName))
        {
            yield return new ValidationResult("Selecione ou informe um responsável técnico.", [nameof(RiggerDisplayName)]);
        }
        else if (RiggerId is null)
        {
            if (string.IsNullOrWhiteSpace(RiggerFullName))
            {
                yield return new ValidationResult("Informe o nome completo do novo responsável.", [nameof(RiggerFullName)]);
            }
            if (string.IsNullOrWhiteSpace(RiggerRppNumber))
            {
                yield return new ValidationResult("Informe o RPP do novo responsável.", [nameof(RiggerRppNumber)]);
            }
        }
    }

    private static bool IsValidCnpj(string? value)
    {
        var normalized = new string((value ?? string.Empty)
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());

        if (normalized.Length != 14) return false;
        return normalized.Take(12).All(char.IsLetterOrDigit)
            && normalized.Skip(12).All(char.IsDigit);
    }
}
