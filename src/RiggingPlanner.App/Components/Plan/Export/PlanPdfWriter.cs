using RiggingPlanner.Application.Export;

namespace RiggingPlanner.App.Components.Plan.Export;

public static class PlanPdfWriter
{
    public static async Task<string> SaveAndOpenAsync(PlanExportDocument document)
    {
        var downloads = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        Directory.CreateDirectory(downloads);

        var filePath = Path.Combine(downloads, document.FileName);
        await File.WriteAllBytesAsync(filePath, document.Pdf);

        await Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(
            new Microsoft.Maui.ApplicationModel.OpenFileRequest(
                "Plano completo", new Microsoft.Maui.Storage.ReadOnlyFile(filePath)));

        return filePath;
    }
}
