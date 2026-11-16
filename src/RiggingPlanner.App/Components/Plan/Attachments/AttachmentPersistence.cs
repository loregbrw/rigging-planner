using Microsoft.EntityFrameworkCore;
using RiggingPlanner.App.Components.Shared;
using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.App.Components.Plan.Attachments;

public static class AttachmentPersistence
{
    public static async Task ApplyAsync(AppDbContext db, PlanDraftState draft, Guid planId, AttachmentSaveResult result)
    {
        var draftPlan = draft.Plan;
        if (draftPlan is null)
        {
            return;
        }

        var plan = await db.RiggingPlans.SingleAsync(x => x.Id == planId);
        var previousArtFileId = plan.ArtFileId;

        await ApplyArtFileAsync(db, plan, draftPlan, result);
        await ApplyDrawingsAsync(db, plan, draftPlan, result);

        if (previousArtFileId is not null && previousArtFileId != plan.ArtFileId)
        {
            await RemoveStoredFileAsync(db, previousArtFileId.Value, result);
        }
    }

    private static async Task ApplyArtFileAsync(AppDbContext db, RiggingPlan plan, RiggingPlan draftPlan,
        AttachmentSaveResult result)
    {
        var artFile = draftPlan.ArtFile;

        if (draftPlan.ArtFileId is not null && artFile is not null && AttachmentDraftState.IsStaged(artFile))
        {
            artFile.RelativePath = CopyToFinalFolder(artFile, "rigging-plan-art", result);
            db.StoredFiles.Add(artFile);
            await db.SaveChangesAsync();

            plan.ArtFile = artFile;
            plan.ArtFileId = artFile.Id;
        }
        else if (draftPlan.ArtFileId is null)
        {
            plan.ArtFile = null;
            plan.ArtFileId = null;
        }

        plan.ArtNumber = draftPlan.ArtNumber;
        await db.SaveChangesAsync();
    }

    private static async Task ApplyDrawingsAsync(AppDbContext db, RiggingPlan plan, RiggingPlan draftPlan,
        AttachmentSaveResult result)
    {
        var persistedDrawings = await db.PlanDrawings
            .Where(x => x.PlanId == plan.Id)
            .ToListAsync();

        var desiredDrawings = draftPlan.Drawings
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToList();

        var desiredIds = desiredDrawings.Select(x => x.Id).ToHashSet();
        var persistedIds = persistedDrawings.Select(x => x.Id).ToHashSet();

        var removedDrawings = persistedDrawings.Where(x => !desiredIds.Contains(x.Id)).ToList();
        if (removedDrawings.Count > 0)
        {
            db.PlanDrawings.RemoveRange(removedDrawings);
            await db.SaveChangesAsync();

            foreach (var drawing in removedDrawings)
            {
                await RemoveStoredFileAsync(db, drawing.FileId, result);
            }
        }

        var nextSortOrder = persistedDrawings.Count == 0
            ? 0
            : persistedDrawings.Max(x => x.SortOrder) + 1;

        var newDrawings = desiredDrawings.Where(x => !persistedIds.Contains(x.Id)).ToList();
        foreach (var drawing in newDrawings)
        {
            var file = drawing.File;

            if (AttachmentDraftState.IsStaged(file))
            {
                file.RelativePath = CopyToFinalFolder(file, "rigging-plan-drawings", result);
                db.StoredFiles.Add(file);
            }
            else if (db.Entry(file).State == EntityState.Detached)
            {
                db.StoredFiles.Attach(file);
            }
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync();
        }

        foreach (var drawing in newDrawings)
        {
            drawing.PlanId = plan.Id;
            drawing.FileId = drawing.File.Id;
            drawing.SortOrder = nextSortOrder++;
            drawing.Plan = plan;
            db.PlanDrawings.Add(drawing);
        }

        await db.SaveChangesAsync();
    }

    private static async Task RemoveStoredFileAsync(AppDbContext db, Guid fileId, AttachmentSaveResult result)
    {
        var file = await db.StoredFiles.SingleOrDefaultAsync(x => x.Id == fileId);
        if (file is null)
        {
            return;
        }

        result.RemovedRelativePaths.Add(file.RelativePath);
        db.StoredFiles.Remove(file);
        await db.SaveChangesAsync();
    }

    private static string CopyToFinalFolder(StoredFile file, string folder, AttachmentSaveResult result)
    {
        var fileName = Path.GetFileName(file.RelativePath);
        var finalRelativePath = Path.Combine(folder, fileName);
        var sourcePath = Path.Combine(FileSystem.AppDataDirectory, file.RelativePath);
        var targetPath = Path.Combine(FileSystem.AppDataDirectory, finalRelativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        File.Copy(sourcePath, targetPath, overwrite: true);
        result.CreatedRelativePaths.Add(finalRelativePath);

        return finalRelativePath;
    }
}

public sealed class AttachmentSaveResult
{
    public List<string> CreatedRelativePaths { get; } = [];
    public List<string> RemovedRelativePaths { get; } = [];

    public void DeleteCreatedFiles() => DeleteAll(CreatedRelativePaths);
    public void DeleteRemovedFiles() => DeleteAll(RemovedRelativePaths);

    private static void DeleteAll(IEnumerable<string> relativePaths)
    {
        foreach (var relativePath in relativePaths)
        {
            try
            {
                var absolutePath = Path.Combine(FileSystem.AppDataDirectory, relativePath);
                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                }
            }
            catch
            {
            }
        }
    }
}
