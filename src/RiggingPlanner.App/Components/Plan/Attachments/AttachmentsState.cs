using Microsoft.AspNetCore.Components.Forms;
using RiggingPlanner.App.Components.Shared;
using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.App.Components.Plan.Attachments;

public sealed class AttachmentsState
{
    private readonly PlanDraftState draft;

    public AttachmentsState(PlanDraftState draft)
    {
        this.draft = draft;
    }

    public Action? Changed { get; set; }
    public RiggingPlan? Plan { get; set; }
    public bool IsReadOnly { get; set; }
    public string? ArtErrorMessage { get; set; }
    public string? DrawingsErrorMessage { get; set; }
    public PlanDrawing? PreviewDrawing { get; private set; }

    public void OpenPreview(PlanDrawing drawing)
    {
        PreviewDrawing = drawing;
        NotifyChanged();
    }

    public void ClosePreview()
    {
        PreviewDrawing = null;
        NotifyChanged();
    }

    public async Task OnArtFileChangeAsync(InputFileChangeEventArgs args)
    {
        if (Plan is null || IsReadOnly) return;
        var browserFile = args.File;
        if (browserFile is null) return;

        if (!string.Equals(browserFile.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) &&
            !browserFile.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            ArtErrorMessage = "Anexe somente arquivos PDF.";
            NotifyChanged();
            return;
        }

        try
        {
            ArtErrorMessage = null;
            var storedFile = await SaveStagedFileAsync(browserFile, 10 * 1024 * 1024, "application/pdf");
            AttachmentDraftState.TryDeleteStagedFile(Plan.ArtFile);
            Plan.ArtFileId = storedFile.Id;
            Plan.ArtFile = storedFile;
        }
        catch (Exception ex)
        {
            ArtErrorMessage = $"Erro ao anexar arquivo: {ex.Message}";
        }
        finally
        {
            NotifyChanged();
        }
    }

    public Task RemoveArtFileAsync()
    {
        if (Plan is null || IsReadOnly) return Task.CompletedTask;

        AttachmentDraftState.TryDeleteStagedFile(Plan.ArtFile);
        Plan.ArtFileId = null;
        Plan.ArtFile = null;
        NotifyChanged();
        return Task.CompletedTask;
    }

    public async Task OnDrawingsChangeAsync(InputFileChangeEventArgs args)
    {
        if (Plan is null || IsReadOnly) return;
        var browserFiles = args.GetMultipleFiles(20);
        if (browserFiles.Count == 0) return;

        try
        {
            DrawingsErrorMessage = null;

            foreach (var browserFile in browserFiles)
            {
                if (!browserFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) &&
                    !IsImageExtension(browserFile.Name))
                {
                    DrawingsErrorMessage = "Por favor, anexe somente arquivos de imagem (PNG, JPG, WEBP, etc.).";
                    continue;
                }

                var storedFile = await SaveStagedFileAsync(browserFile, 20 * 1024 * 1024, "image/png");
                var drawing = new PlanDrawing
                {
                    Id = Guid.NewGuid(),
                    PlanId = Plan.Id,
                    FileId = storedFile.Id,
                    SortOrder = NextSortOrder(),
                    File = storedFile
                };

                Plan.Drawings.Add(drawing);
            }
        }
        catch (Exception ex)
        {
            DrawingsErrorMessage = $"Erro ao anexar imagens: {ex.Message}";
        }
        finally
        {
            NotifyChanged();
        }
    }

    public Task RemoveDrawingAsync(PlanDrawing drawing)
    {
        if (Plan is null || IsReadOnly) return Task.CompletedTask;

        Plan.Drawings.Remove(drawing);
        AttachmentDraftState.TryDeleteStagedFile(drawing.File);

        if (PreviewDrawing?.Id == drawing.Id)
        {
            PreviewDrawing = null;
        }

        NotifyChanged();
        return Task.CompletedTask;
    }

    public static string GetFileAsDataUri(StoredFile? file)
    {
        if (file is null) return string.Empty;
        try
        {
            var absolutePath = Path.Combine(FileSystem.AppDataDirectory, file.RelativePath);
            if (File.Exists(absolutePath))
            {
                var bytes = File.ReadAllBytes(absolutePath);
                var mime = string.IsNullOrWhiteSpace(file.ContentType) ? "image/png" : file.ContentType;
                return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
            }
        }
        catch { }
        return string.Empty;
    }

    public static string FormatByteSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:N1} KB";
        return $"{bytes / (1024.0 * 1024.0):N1} MB";
    }

    private void NotifyChanged() => Changed?.Invoke();

    private int NextSortOrder()
        => Plan is null || Plan.Drawings.Count == 0 ? 0 : Plan.Drawings.Max(x => x.SortOrder) + 1;

    private static bool IsImageExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return ext.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".webp", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<StoredFile> SaveStagedFileAsync(IBrowserFile browserFile, long maxSize, string fallbackContentType)
    {
        var directory = draft.Attachments.EnsureStagingDirectory();
        var fileId = Guid.NewGuid();
        var extension = Path.GetExtension(browserFile.Name);
        var fileName = fileId + extension;
        var absolutePath = Path.Combine(directory, fileName);

        await using (var input = browserFile.OpenReadStream(maxSize))
        await using (var output = File.Create(absolutePath))
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var buffer = new byte[81920];
            int read;
            long size = 0;
            while ((read = await input.ReadAsync(buffer)) > 0)
            {
                await output.WriteAsync(buffer.AsMemory(0, read));
                sha256.TransformBlock(buffer, 0, read, null, 0);
                size += read;
            }
            sha256.TransformFinalBlock([], 0, 0);
            var hash = Convert.ToHexString(sha256.Hash ?? []).ToLowerInvariant();

            return new StoredFile
            {
                Id = fileId,
                RelativePath = draft.Attachments.ToRelativeStagingPath(fileName),
                OriginalName = browserFile.Name,
                ContentType = string.IsNullOrWhiteSpace(browserFile.ContentType) ? fallbackContentType : browserFile.ContentType,
                SizeBytes = size,
                Sha256 = hash,
                CreatedAt = DateTime.Now
            };
        }
    }
}
