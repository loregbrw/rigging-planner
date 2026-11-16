using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.App.Components.Plan.Attachments;

public sealed class AttachmentsState
{
    private readonly IDbContextFactory<AppDbContext> dbFactory;

    public AttachmentsState(IDbContextFactory<AppDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public Action? Changed { get; set; }
    public RiggingPlan? Plan { get; set; }
    public bool IsPersisted { get; set; }
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
            var storedFile = await SaveFileAsync(browserFile);

            await using var db = await dbFactory.CreateDbContextAsync();
            db.StoredFiles.Add(storedFile);
            Plan.ArtFileId = storedFile.Id;
            Plan.ArtFile = storedFile;

            if (IsPersisted)
            {
                var dbPlan = await db.RiggingPlans.FindAsync(Plan.Id);
                if (dbPlan is not null)
                {
                    dbPlan.ArtFileId = storedFile.Id;
                    dbPlan.ArtNumber = Plan.ArtNumber;
                }
            }
            await db.SaveChangesAsync();
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

    public async Task RemoveArtFileAsync()
    {
        if (Plan is null || IsReadOnly) return;
        Plan.ArtFileId = null;
        Plan.ArtFile = null;

        await using var db = await dbFactory.CreateDbContextAsync();
        if (IsPersisted)
        {
            var dbPlan = await db.RiggingPlans.FindAsync(Plan.Id);
            if (dbPlan is not null)
            {
                dbPlan.ArtFileId = null;
                await db.SaveChangesAsync();
            }
        }
        else
        {
            await db.SaveChangesAsync();
        }
        NotifyChanged();
    }

    public async Task OnDrawingsChangeAsync(InputFileChangeEventArgs args)
    {
        if (Plan is null || IsReadOnly) return;
        var browserFiles = args.GetMultipleFiles(20);
        if (browserFiles.Count == 0) return;

        try
        {
            DrawingsErrorMessage = null;
            await using var db = await dbFactory.CreateDbContextAsync();

            foreach (var browserFile in browserFiles)
            {
                if (!browserFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) &&
                    !IsImageExtension(browserFile.Name))
                {
                    DrawingsErrorMessage = "Por favor, anexe somente arquivos de imagem (PNG, JPG, WEBP, etc.).";
                    continue;
                }

                var storedFile = await SaveDrawingFileAsync(browserFile);
                db.StoredFiles.Add(storedFile);

                var drawing = new PlanDrawing
                {
                    Id = Guid.NewGuid(),
                    PlanId = Plan.Id,
                    FileId = storedFile.Id,
                    SortOrder = Plan.Drawings.Count,
                    File = storedFile
                };

                db.PlanDrawings.Add(drawing);
                Plan.Drawings.Add(drawing);
            }

            await db.SaveChangesAsync();
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

    public async Task RemoveDrawingAsync(PlanDrawing drawing)
    {
        if (Plan is null || IsReadOnly) return;
        Plan.Drawings.Remove(drawing);

        await using var db = await dbFactory.CreateDbContextAsync();
        if (IsPersisted)
        {
            var dbDrawing = await db.PlanDrawings.Include(x => x.File).SingleOrDefaultAsync(x => x.Id == drawing.Id);
            if (dbDrawing is not null)
            {
                db.PlanDrawings.Remove(dbDrawing);
                db.StoredFiles.Remove(dbDrawing.File);
                await db.SaveChangesAsync();
            }
        }
        else
        {
            await db.SaveChangesAsync();
        }
        NotifyChanged();
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

    private static bool IsImageExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return ext.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".webp", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<StoredFile> SaveDrawingFileAsync(IBrowserFile browserFile)
    {
        var fileId = Guid.NewGuid();
        var extension = Path.GetExtension(browserFile.Name);
        var folder = Path.Combine(FileSystem.AppDataDirectory, "rigging-plan-drawings");
        Directory.CreateDirectory(folder);
        var relativePath = Path.Combine("rigging-plan-drawings", fileId + extension);
        var absolutePath = Path.Combine(FileSystem.AppDataDirectory, relativePath);

        await using var input = browserFile.OpenReadStream(20 * 1024 * 1024);
        await using var output = File.Create(absolutePath);
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
            RelativePath = relativePath,
            OriginalName = browserFile.Name,
            ContentType = string.IsNullOrWhiteSpace(browserFile.ContentType) ? "image/png" : browserFile.ContentType,
            SizeBytes = size,
            Sha256 = hash,
            CreatedAt = DateTime.Now
        };
    }

    private static async Task<StoredFile> SaveFileAsync(IBrowserFile browserFile)
    {
        var fileId = Guid.NewGuid();
        var extension = Path.GetExtension(browserFile.Name);
        var folder = Path.Combine(FileSystem.AppDataDirectory, "rigging-plan-art");
        Directory.CreateDirectory(folder);
        var relativePath = Path.Combine("rigging-plan-art", fileId + extension);
        var absolutePath = Path.Combine(FileSystem.AppDataDirectory, relativePath);

        await using var input = browserFile.OpenReadStream(10 * 1024 * 1024);
        await using var output = File.Create(absolutePath);
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
            RelativePath = relativePath,
            OriginalName = browserFile.Name,
            ContentType = browserFile.ContentType,
            SizeBytes = size,
            Sha256 = hash,
            CreatedAt = DateTime.Now
        };
    }
}
