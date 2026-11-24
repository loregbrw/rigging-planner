using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Application.Catalog;

public sealed record ClientSaveRequest
{
    public Guid? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Cnpj { get; init; } = string.Empty;
}

public sealed record EquipmentSaveRequest
{
    public Guid? Id { get; init; }
    public string CraneModel { get; init; } = string.Empty;
    public string TruckModel { get; init; } = string.Empty;
    public string ChassisNumber { get; init; } = string.Empty;
    public string LicensePlate { get; init; } = string.Empty;
    public int? ManufactureYear { get; init; }
}

public sealed record RiggerSaveRequest
{
    public Guid? Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string RppNumber { get; init; } = string.Empty;
    public string? CreaNumber { get; init; }
}

public sealed record CatalogSaveResult(string? Error, bool Created);

public sealed record EquipmentSaveResult(string? Error, Guid? EquipmentId, bool Created)
{
    public bool Success => Error is null && EquipmentId is not null;
}

public static class CatalogMessages
{
    public const string InvalidCnpj = "Informe um CNPJ válido no formato 00.000.000/0000-00 ou 00.000.000/E08G-12.";
    public const string DuplicateCnpj = "Já existe um cliente cadastrado com este CNPJ.";
    public const string ClientNotFound = "O cliente não foi encontrado.";
    public const string EquipmentNotFound = "O equipamento não foi encontrado.";
    public const string RiggerNotFound = "O rigger não foi encontrado.";
    public const string ClientInUse = "Não é possível excluir: o cliente está ligado a um plano em andamento.";
    public const string EquipmentInUse = "Não é possível excluir: o equipamento está ligado a um plano em andamento.";
    public const string RiggerInUse = "Não é possível excluir: o responsável está ligado a um plano em andamento.";
    public const string ClientCreated = "Cliente cadastrado com sucesso.";
    public const string ClientUpdated = "Cliente atualizado com sucesso.";
    public const string EquipmentCreated = "Equipamento e ART inicial cadastrados com sucesso.";
    public const string EquipmentUpdated = "Equipamento atualizado com sucesso.";
    public const string RiggerCreated = "Responsável cadastrado com sucesso.";
    public const string RiggerUpdated = "Responsável atualizado com sucesso.";
}

public interface ICatalogService
{
    Task<IReadOnlyList<Client>> GetClientsAsync(bool activeOnly = false);
    Task<IReadOnlyList<Equipment>> GetEquipmentsAsync(bool activeOnly = false, bool includeReports = false);
    Task<IReadOnlyList<Rigger>> GetRiggersAsync(bool activeOnly = false);
    Task<Client?> FindClientAsync(Guid id);
    Task<Equipment?> FindEquipmentAsync(Guid id);
    Task<Rigger?> FindRiggerAsync(Guid id);

    Task<CatalogSaveResult> SaveClientAsync(ClientSaveRequest request);
    Task<EquipmentSaveResult> SaveEquipmentAsync(EquipmentSaveRequest request);
    Task<CatalogSaveResult> SaveRiggerAsync(RiggerSaveRequest request);

    Task ToggleClientStatusAsync(Guid id);
    Task ToggleEquipmentStatusAsync(Guid id);
    Task ToggleRiggerStatusAsync(Guid id);

    Task<string?> DeleteClientAsync(Guid id);
    Task<string?> DeleteEquipmentAsync(Guid id);
    Task<string?> DeleteRiggerAsync(Guid id);
}
