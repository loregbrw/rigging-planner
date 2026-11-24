using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Application.EquipmentArts;

public sealed record EquipmentArtReport
{
    public Guid Id { get; init; }
    public string ArtNumber { get; init; } = string.Empty;
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
    public string? FileName { get; init; }
}

public sealed record EquipmentArtSaveRequest
{
    public Guid? Id { get; init; }
    public Guid EquipmentId { get; init; }
    public string ArtNumber { get; init; } = string.Empty;
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
    public string? FileName { get; init; }
    public string? ContentType { get; init; }
    public byte[]? PdfContent { get; init; }
}

public sealed record EquipmentArtValidity
{
    public EquipmentReport? Report { get; init; }
    public bool IsValid { get; init; }
}

public static class EquipmentArtMessages
{
    public const string ReportNotFound = "A ART não foi encontrada.";
    public const string Added = "ART adicionada com sucesso.";
    public const string Updated = "ART atualizada com sucesso.";
    public const string Deleted = "ART excluída com sucesso.";

    public static string InUse(string artNumber) =>
        $"A ART {artNumber} está vinculada a um plano e não pode ser excluída.";
}

public interface IEquipmentArtService
{
    Task<IReadOnlyList<EquipmentArtReport>> ListAsync(Guid equipmentId);
    Task<string?> SaveAsync(EquipmentArtSaveRequest request);
    Task<string?> DeleteAsync(Guid reportId);
    Task<EquipmentArtValidity> ResolveValidityAsync(Guid equipmentId, DateTime planDate);
}
