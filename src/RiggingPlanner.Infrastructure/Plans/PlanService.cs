using Microsoft.EntityFrameworkCore;
using RiggingPlanner.Application.Abstractions;
using RiggingPlanner.Application.Catalog;
using RiggingPlanner.Application.Plans;
using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Domain.Services;
using RiggingPlanner.Domain.ValueObjects;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.Infrastructure.Plans;

public sealed class PlanService(
    IDbContextFactory<AppDbContext> dbFactory,
    IFileStore fileStore) : IPlanService
{
    private const string StagingRoot = "rigging-plan-staging";

    public async Task<PlanSaveResult> SaveAsync(PlanSaveRequest request)
    {
        var validationErrors = PlanFormValidator.Validate(request.Form);
        if (validationErrors.Count > 0)
        {
            return new PlanSaveResult(null, validationErrors, null);
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var files = new AttachmentFileChanges(fileStore);

        var result = await SaveCoreAsync(db, request, files);
        if (!result.Success)
        {
            files.DeleteCreatedFiles();
            await transaction.RollbackAsync();
            return result;
        }

        await transaction.CommitAsync();
        files.DeleteRemovedFiles();
        return result;
    }

    public async Task<PlanFinalizeResult> FinalizeAsync(PlanSaveRequest request, DateTime? finalizedAt = null)
    {
        var validationErrors = PlanFormValidator.Validate(request.Form);
        if (validationErrors.Count > 0)
        {
            return new PlanFinalizeResult(null, null, validationErrors);
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var files = new AttachmentFileChanges(fileStore);

        var currentPlanId = request.PlanId;
        if (currentPlanId is not null)
        {
            var existing = await db.RiggingPlans
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == currentPlanId && x.DeletedAt == null);
            if (existing is null)
            {
                return new PlanFinalizeResult("Plano não encontrado.", null, []);
            }
            if (existing.Status == "finalized")
            {
                return new PlanFinalizeResult("O plano já está finalizado.", null, []);
            }
        }

        var saved = await SaveCoreAsync(db, request, files);
        if (!saved.Success)
        {
            files.DeleteCreatedFiles();
            await transaction.RollbackAsync();
            return new PlanFinalizeResult(saved.Error, saved.PlanId, saved.ValidationErrors);
        }

        var planId = saved.PlanId ?? currentPlanId;
        if (planId is null)
        {
            files.DeleteCreatedFiles();
            await transaction.RollbackAsync();
            return new PlanFinalizeResult("Plano não encontrado.", null, []);
        }

        var plan = await db.RiggingPlans
            .Include(x => x.ProcedureSnapshot)
            .SingleOrDefaultAsync(x => x.Id == planId && x.DeletedAt == null);
        if (plan is null)
        {
            files.DeleteCreatedFiles();
            await transaction.RollbackAsync();
            return new PlanFinalizeResult("Plano não encontrado.", null, []);
        }
        if (plan.Status == "finalized")
        {
            files.DeleteCreatedFiles();
            await transaction.RollbackAsync();
            return new PlanFinalizeResult("O plano já está finalizado.", null, []);
        }

        var date = finalizedAt ?? DateTime.Now;
        plan.Status = "finalized";
        plan.FinalizedAt = date;
        plan.UpdatedAt = date;

        var snapshot = OperationalProcedureService.CreateSnapshot(plan, date);
        var stored = await db.PlanProcedureSnapshots.SingleOrDefaultAsync(x => x.PlanId == planId);
        if (stored is null)
        {
            db.PlanProcedureSnapshots.Add(snapshot);
        }
        else
        {
            db.Entry(stored).CurrentValues.SetValues(snapshot);
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        files.DeleteRemovedFiles();
        return new PlanFinalizeResult(null, planId, []);
    }

    public async Task<RiggingPlan?> LoadPlanAsync(Guid planId, bool includeAccessories = false,
        bool includeAttachments = false)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        IQueryable<RiggingPlan> query = db.RiggingPlans
            .AsNoTracking()
            .Include(x => x.ArtFile)
            .Include(x => x.Drawings).ThenInclude(x => x.File);
        if (includeAttachments)
        {
            query = query
                .Include(x => x.EquipmentReport).ThenInclude(x => x!.File)
                .Include(x => x.ProcedureSnapshot);
        }

        var plan = await query.SingleOrDefaultAsync(x => x.Id == planId && x.DeletedAt == null);
        if (plan is null)
        {
            return null;
        }

        if (includeAccessories)
        {
            var accessories = await db.PlanAccessories
                .AsNoTracking()
                .Where(x => x.PlanId == planId)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
            foreach (var accessory in accessories)
            {
                plan.Accessories.Add(accessory);
            }
        }

        return plan;
    }

    public async Task<IReadOnlyList<PlanAccessory>> GetAccessoriesAsync(Guid planId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.PlanAccessories
            .AsNoTracking()
            .Where(x => x.PlanId == planId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<RiggingPlan>> ListPlansAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.RiggingPlans
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    private async Task<PlanSaveResult> SaveCoreAsync(AppDbContext db, PlanSaveRequest request,
        AttachmentFileChanges files)
    {
        try
        {
            var form = request.Form;
            var equipmentOptions = request.EquipmentOptions.ToList();
            var riggerOptions = request.RiggerOptions.ToList();

            var clientResult = await ResolveClientAsync(db, form);
            if (clientResult.Error is not null)
            {
                return new PlanSaveResult(clientResult.Error, [], null);
            }

            var client = clientResult.Client;
            var equipment = await ResolveEquipmentAsync(db, form, equipmentOptions);
            var rigger = await ResolveRiggerAsync(db, form, riggerOptions);

            if (request.PlanId is not null && !request.IsDraftPlan)
            {
                var existingPlan = await db.RiggingPlans
                    .Include(x => x.ArtFile)
                    .Include(x => x.Drawings)
                    .SingleOrDefaultAsync(x => x.Id == request.PlanId);
                if (existingPlan is null)
                {
                    return new PlanSaveResult("Plano não encontrado.", [], null);
                }

                PlanFormMapper.ApplyToSavedPlan(form, existingPlan, client, equipment, rigger);
                CopyCapacityFields(request.CapacitySource, existingPlan);
                ApplyReferences(existingPlan, equipmentOptions, riggerOptions);
                existingPlan.UpdatedAt = DateTime.Now;
                await db.SaveChangesAsync();
                await SaveAccessoriesAsync(db, request.Accessories, existingPlan.Id);
                await ApplyAttachmentsAsync(db, request.AttachmentSource, existingPlan.Id, files);
                return new PlanSaveResult(null, [], existingPlan.Id);
            }

            var yearMonth = PlanNumber.GetYearMonth(form.PlanDate);
            var counter = await db.PlanNumberCounters.SingleOrDefaultAsync(x => x.YearMonth == yearMonth);
            if (counter is null)
            {
                counter = new PlanNumberCounter { YearMonth = yearMonth, LastSequence = 0 };
                db.PlanNumberCounters.Add(counter);
            }
            var nextNumber = PlanNumber.Next(yearMonth, counter.LastSequence);
            counter.LastSequence = nextNumber.Sequence;

            var newPlan = new RiggingPlan
            {
                Id = Guid.NewGuid(),
                PlanNumber = nextNumber.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Client = client,
                Rigger = rigger
            };
            PlanFormMapper.ApplyToSavedPlan(form, newPlan, client, equipment, rigger, formatCnpj: false);
            CopyCapacityFields(request.CapacitySource, newPlan);
            ApplyReferences(newPlan, equipmentOptions, riggerOptions);
            db.RiggingPlans.Add(newPlan);

            await db.SaveChangesAsync();
            await SaveAccessoriesAsync(db, request.Accessories, newPlan.Id);
            await ApplyAttachmentsAsync(db, request.AttachmentSource, newPlan.Id, files);
            return new PlanSaveResult(null, [], newPlan.Id);
        }
        catch
        {
            files.DeleteCreatedFiles();
            throw;
        }
    }

    private static async Task<(Client? Client, string? Error)> ResolveClientAsync(AppDbContext db, PlanFormModel form)
    {
        if (form.ClientId is not null)
        {
            return (await db.Clients.SingleOrDefaultAsync(x => x.Id == form.ClientId), null);
        }

        if (string.IsNullOrWhiteSpace(form.ClientName))
        {
            return (null, null);
        }

        var cnpj = CnpjFormatter.Format(form.ClientCnpj);
        if (cnpj is null || CnpjFormatter.Normalize(cnpj)?.Length != 14)
        {
            return (null, "Informe um CNPJ válido no formato 00.000.000/0000-00 ou 00.000.000/E08G-12.");
        }

        var normalized = CnpjFormatter.Normalize(cnpj);
        if ((await db.Clients.AsNoTracking().Select(x => x.Cnpj).ToListAsync())
            .Any(value => CnpjFormatter.Normalize(value) == normalized))
        {
            return (null, "Já existe um cliente cadastrado com este CNPJ. Selecione-o na lista.");
        }

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = form.ClientName.Trim(),
            Cnpj = cnpj,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        db.Clients.Add(client);
        return (client, null);
    }

    private static async Task<Equipment?> ResolveEquipmentAsync(AppDbContext db, PlanFormModel form,
        List<Equipment> equipmentOptions)
    {
        if (form.EquipmentId is not null)
        {
            return await db.Equipments.SingleOrDefaultAsync(x => x.Id == form.EquipmentId);
        }

        if (string.IsNullOrWhiteSpace(form.EquipmentModel))
        {
            return null;
        }

        var existing = await db.Equipments.SingleOrDefaultAsync(x =>
            x.CraneModel.ToLower() == form.EquipmentModel.Trim().ToLower());
        if (existing is not null)
        {
            form.EquipmentId = existing.Id;
            return existing;
        }

        var equipment = new Equipment
        {
            Id = Guid.NewGuid(),
            CraneModel = form.EquipmentModel.Trim(),
            TruckModel = form.EquipmentTruckModel.Trim(),
            ChassisNumber = form.EquipmentChassisNumber.Trim(),
            LicensePlate = form.EquipmentLicensePlate.Trim().ToUpperInvariant(),
            ManufactureYear = form.EquipmentManufactureYear,
            EquipmentType = "munck",
            CraneBrand = string.Empty,
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        db.Equipments.Add(equipment);
        form.EquipmentId = equipment.Id;
        equipmentOptions.Add(equipment);
        return equipment;
    }

    private static async Task<Rigger?> ResolveRiggerAsync(AppDbContext db, PlanFormModel form,
        List<Rigger> riggerOptions)
    {
        if (form.RiggerId is not null)
        {
            return await db.Riggers.SingleOrDefaultAsync(x => x.Id == form.RiggerId);
        }

        if (string.IsNullOrWhiteSpace(form.RiggerDisplayName))
        {
            return null;
        }

        var existing = await db.Riggers.SingleOrDefaultAsync(x =>
            x.DisplayName.ToLower() == form.RiggerDisplayName.Trim().ToLower());
        if (existing is not null)
        {
            form.RiggerId = existing.Id;
            return existing;
        }

        var rigger = new Rigger
        {
            Id = Guid.NewGuid(),
            FullName = string.IsNullOrWhiteSpace(form.RiggerFullName)
                ? form.RiggerDisplayName.Trim()
                : form.RiggerFullName.Trim(),
            DisplayName = form.RiggerDisplayName.Trim(),
            RppNumber = string.IsNullOrWhiteSpace(form.RiggerRppNumber) ? "000" : form.RiggerRppNumber.Trim(),
            CreaNumber = string.IsNullOrWhiteSpace(form.RiggerCreaNumber) ? null : form.RiggerCreaNumber.Trim(),
            IsActive = true
        };
        db.Riggers.Add(rigger);
        form.RiggerId = rigger.Id;
        riggerOptions.Add(rigger);
        return rigger;
    }

    private static void ApplyReferences(RiggingPlan plan, IReadOnlyCollection<Equipment> equipmentOptions,
        IReadOnlyCollection<Rigger> riggerOptions)
    {
        var equipment = plan.EquipmentId is null ? null : equipmentOptions.FirstOrDefault(x => x.Id == plan.EquipmentId);
        if (equipment is not null)
        {
            plan.EquipmentId = equipment.Id;
            plan.TruckModel = equipment.TruckModel;
            plan.LicensePlate = equipment.LicensePlate;
            plan.ChassisNumber = equipment.ChassisNumber;
            plan.ManufactureYear = equipment.ManufactureYear;
            plan.CraneBrand = equipment.CraneBrand;
            plan.CraneModel = equipment.CraneModel;
        }

        var rigger = plan.RiggerId is null ? null : riggerOptions.FirstOrDefault(x => x.Id == plan.RiggerId);
        if (rigger is not null)
        {
            plan.RiggerId = rigger.Id;
            plan.RiggerName = rigger.DisplayName;
            plan.RiggerRpp = rigger.RppNumber;
            plan.RiggerCrea = rigger.CreaNumber;
        }
    }

    private static async Task SaveAccessoriesAsync(AppDbContext db, IReadOnlyList<PlanAccessory> accessories,
        Guid planId)
    {
        var storedItems = await db.PlanAccessories.Where(x => x.PlanId == planId).ToListAsync();
        if (storedItems.Count > 0)
        {
            db.PlanAccessories.RemoveRange(storedItems);
        }
        foreach (var item in accessories)
        {
            item.PlanId = planId;
            item.Plan = null!;
            db.PlanAccessories.Add(item);
        }
        if (storedItems.Count > 0 || accessories.Count > 0)
        {
            await db.SaveChangesAsync();
        }
    }

    private static void CopyCapacityFields(RiggingPlan? source, RiggingPlan target)
    {
        if (source is null)
        {
            return;
        }
        target.CraneNominalCapacity = source.CraneNominalCapacity;
        target.BoomLengthM = source.BoomLengthM;
        target.ExtraLoadKg = source.ExtraLoadKg;
        target.OperatingRadiusM = source.OperatingRadiusM;
        target.LoadCapacityT = source.LoadCapacityT;
        target.QuadrantStart = source.QuadrantStart;
        target.QuadrantEnd = source.QuadrantEnd;
        target.ArtNumber = source.ArtNumber;
        target.FlatStrapUtilizationPct = source.FlatStrapUtilizationPct;
        target.TubularStrapUtilizationPct = source.TubularStrapUtilizationPct;
        target.WireRopeSlingUtilizationPct = source.WireRopeSlingUtilizationPct;
        target.ShackleUtilizationPct = source.ShackleUtilizationPct;
        target.FootingForceKgf = source.FootingForceKgf;
        target.MinGroundResistanceKgfCm2 = source.MinGroundResistanceKgfCm2;
        target.MaxWindSpeedMs = source.MaxWindSpeedMs;
    }

    private async Task ApplyAttachmentsAsync(AppDbContext db, RiggingPlan? draftPlan, Guid planId,
        AttachmentFileChanges files)
    {
        if (draftPlan is null)
        {
            return;
        }

        var plan = await db.RiggingPlans.SingleAsync(x => x.Id == planId);
        var previousArtFileId = plan.ArtFileId;

        await ApplyArtFileAsync(db, plan, draftPlan, files);
        await ApplyDrawingsAsync(db, plan, draftPlan, files);

        if (previousArtFileId is not null && previousArtFileId != plan.ArtFileId)
        {
            await RemoveStoredFileAsync(db, previousArtFileId.Value, files);
        }
    }

    private async Task ApplyArtFileAsync(AppDbContext db, RiggingPlan plan, RiggingPlan draftPlan,
        AttachmentFileChanges files)
    {
        var artFile = draftPlan.ArtFile;

        if (draftPlan.ArtFileId is not null && artFile is not null && IsStaged(artFile))
        {
            artFile.RelativePath = CopyToFinalFolder(artFile, "rigging-plan-art", files);
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

    private async Task ApplyDrawingsAsync(AppDbContext db, RiggingPlan plan, RiggingPlan draftPlan,
        AttachmentFileChanges files)
    {
        var persistedDrawings = await db.PlanDrawings.Where(x => x.PlanId == plan.Id).ToListAsync();

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
                await RemoveStoredFileAsync(db, drawing.FileId, files);
            }
        }

        var nextSortOrder = persistedDrawings.Count == 0
            ? 0
            : persistedDrawings.Max(x => x.SortOrder) + 1;

        var newDrawings = desiredDrawings.Where(x => !persistedIds.Contains(x.Id)).ToList();
        foreach (var drawing in newDrawings)
        {
            var file = drawing.File;

            if (IsStaged(file))
            {
                file.RelativePath = CopyToFinalFolder(file, "rigging-plan-drawings", files);
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

    private static async Task RemoveStoredFileAsync(AppDbContext db, Guid fileId, AttachmentFileChanges files)
    {
        var file = await db.StoredFiles.SingleOrDefaultAsync(x => x.Id == fileId);
        if (file is null)
        {
            return;
        }

        files.RemovedRelativePaths.Add(file.RelativePath);
        db.StoredFiles.Remove(file);
        await db.SaveChangesAsync();
    }

    private static bool IsStaged(StoredFile? file)
        => file is not null && file.RelativePath.StartsWith(StagingRoot, StringComparison.OrdinalIgnoreCase);

    private string CopyToFinalFolder(StoredFile file, string folder, AttachmentFileChanges files)
    {
        var fileName = Path.GetFileName(file.RelativePath);
        var finalRelativePath = Path.Combine(folder, fileName);
        fileStore.CopyTo(file.RelativePath, finalRelativePath);
        files.CreatedRelativePaths.Add(finalRelativePath);
        return finalRelativePath;
    }

    private sealed class AttachmentFileChanges(IFileStore fileStore)
    {
        public List<string> CreatedRelativePaths { get; } = [];
        public List<string> RemovedRelativePaths { get; } = [];

        public void DeleteCreatedFiles() => DeleteAll(CreatedRelativePaths);

        public void DeleteRemovedFiles() => DeleteAll(RemovedRelativePaths);

        private void DeleteAll(IEnumerable<string> relativePaths)
        {
            foreach (var relativePath in relativePaths)
            {
                try
                {
                    fileStore.Delete(relativePath);
                }
                catch
                {
                }
            }
        }
    }
}
