using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using RiggingPlanner.Application.Abstractions;
using RiggingPlanner.Application.EquipmentArts;
using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.Infrastructure.EquipmentArts;

public sealed class EquipmentArtService(
    IDbContextFactory<AppDbContext> dbFactory,
    IFileStore fileStore) : IEquipmentArtService
{
    public async Task<IReadOnlyList<EquipmentArtReport>> ListAsync(Guid equipmentId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.EquipmentReports
            .AsNoTracking()
            .Include(x => x.File)
            .Where(x => x.EquipmentId == equipmentId)
            .OrderByDescending(x => x.ValidFrom)
            .Select(x => new EquipmentArtReport
            {
                Id = x.Id,
                ArtNumber = x.ArtNumber,
                ValidFrom = x.ValidFrom,
                ValidTo = x.ValidTo,
                FileName = x.File.OriginalName
            })
            .ToListAsync();
    }

    public async Task<string?> SaveAsync(EquipmentArtSaveRequest request)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        if (request.Id is null)
        {
            var file = CreateStoredFile(request);
            await fileStore.WriteAsync(file.RelativePath, request.PdfContent ?? []);
            db.StoredFiles.Add(file);
            db.EquipmentReports.Add(new EquipmentReport
            {
                Id = Guid.NewGuid(),
                EquipmentId = request.EquipmentId,
                FileId = file.Id,
                ArtNumber = request.ArtNumber.Trim(),
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                CreatedAt = DateTime.Now
            });
            try
            {
                await db.SaveChangesAsync();
            }
            catch
            {
                fileStore.Delete(file.RelativePath);
                throw;
            }
            return null;
        }

        var report = await db.EquipmentReports
            .Include(x => x.File)
            .SingleOrDefaultAsync(x => x.Id == request.Id.Value);
        if (report is null)
        {
            return EquipmentArtMessages.ReportNotFound;
        }

        report.ArtNumber = request.ArtNumber.Trim();
        report.ValidFrom = request.ValidFrom;
        report.ValidTo = request.ValidTo;

        StoredFile? newFile = null;
        StoredFile? oldFile = null;
        string? previousPath = null;
        if (request.PdfContent is not null)
        {
            oldFile = report.File;
            previousPath = oldFile?.RelativePath;
            newFile = CreateStoredFile(request);
            await fileStore.WriteAsync(newFile.RelativePath, request.PdfContent);
            report.File = newFile;
            db.StoredFiles.Add(newFile);
            if (oldFile is not null)
            {
                db.StoredFiles.Remove(oldFile);
            }
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch
        {
            if (newFile is not null)
            {
                fileStore.Delete(newFile.RelativePath);
            }
            throw;
        }

        if (previousPath is not null)
        {
            fileStore.Delete(previousPath);
        }
        return null;
    }

    public async Task<string?> DeleteAsync(Guid reportId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var report = await db.EquipmentReports
            .Include(x => x.File)
            .SingleOrDefaultAsync(x => x.Id == reportId);
        if (report is null)
        {
            return null;
        }

        if (await db.RiggingPlans.AnyAsync(x => x.EquipmentReportId == report.Id && x.DeletedAt == null))
        {
            return EquipmentArtMessages.InUse(report.ArtNumber);
        }

        var relativePath = report.File?.RelativePath;
        db.EquipmentReports.Remove(report);
        if (report.File is not null)
        {
            db.StoredFiles.Remove(report.File);
        }
        await db.SaveChangesAsync();
        if (relativePath is not null)
        {
            fileStore.Delete(relativePath);
        }
        return null;
    }

    public async Task<EquipmentArtValidity> ResolveValidityAsync(Guid equipmentId, DateTime planDate)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var valid = await db.EquipmentReports
            .AsNoTracking()
            .Include(x => x.File)
            .Where(x => x.EquipmentId == equipmentId && (x.ValidTo == null || x.ValidTo >= planDate))
            .OrderByDescending(x => x.ValidFrom)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
        if (valid is not null)
        {
            return new EquipmentArtValidity { Report = valid, IsValid = true };
        }

        var latest = await db.EquipmentReports
            .AsNoTracking()
            .Include(x => x.File)
            .Where(x => x.EquipmentId == equipmentId)
            .OrderByDescending(x => x.ValidFrom)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
        return new EquipmentArtValidity { Report = latest, IsValid = false };
    }

    private static StoredFile CreateStoredFile(EquipmentArtSaveRequest request)
    {
        var fileId = Guid.NewGuid();
        var extension = Path.GetExtension(request.FileName ?? string.Empty);
        return new StoredFile
        {
            Id = fileId,
            RelativePath = Path.Combine("equipment-art", fileId + extension),
            OriginalName = request.FileName ?? string.Empty,
            ContentType = request.ContentType ?? string.Empty,
            SizeBytes = request.PdfContent?.LongLength ?? 0,
            Sha256 = Convert.ToHexString(SHA256.HashData(request.PdfContent ?? [])).ToLowerInvariant(),
            CreatedAt = DateTime.Now
        };
    }
}
