using Microsoft.AspNetCore.Components.Forms;

namespace RiggingPlanner.App.Components.Shared;

public sealed class EquipmentArtDraft
{
    public Guid? Id { get; set; }
    public string ArtNumber { get; set; } = string.Empty;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public IBrowserFile? File { get; set; }
    public string? ExistingFileName { get; set; }
}
