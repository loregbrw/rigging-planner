using Microsoft.EntityFrameworkCore;
using RiggingPlanner.App.Components.Plan.Attachments;
using RiggingPlanner.App.Components.Plan.Shared;
using RiggingPlanner.App.Components.Shared;
using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.App.Components.Plan.Entry;

public static class PlanEntryPersistence
{
    public static async Task<string?> SaveAsync(AppDbContext db, PlanDraftState draft, Guid? planId, bool isDraftPlan,
        PlanEntryState state)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var error = await SaveCoreAsync(db, draft, planId, isDraftPlan, state);
        if (error is not null)
        {
            await transaction.RollbackAsync();
            return error;
        }

        await transaction.CommitAsync();
        return null;
    }

    public static async Task<string?> SaveCoreAsync(AppDbContext db, PlanDraftState draft, Guid? planId, bool isDraftPlan,
        PlanEntryState state)
    {
        var form = draft.Form;
        var attachmentResult = new AttachmentSaveResult();

        try
        {
            var client = await ResolveClientAsync(db, form);
            var equipment = await ResolveEquipmentAsync(db, form, state);
            var rigger = await ResolveRiggerAsync(db, form, state);

            if (planId is not null && !isDraftPlan)
            {
                var existingPlan = await db.RiggingPlans
                    .Include(x => x.ArtFile)
                    .Include(x => x.Drawings)
                    .SingleOrDefaultAsync(x => x.Id == planId);
                if (existingPlan is null)
                {
                    return "Plano não encontrado.";
                }

                PlanFormMapper.ApplyToSavedPlan(form, existingPlan, client, equipment, rigger);
                CopyCapacityFields(draft.Plan, existingPlan);
                ApplyReferences(existingPlan, state.EquipmentOptions, state.RiggerOptions);
                existingPlan.UpdatedAt = DateTime.Now;
                await db.SaveChangesAsync();
                await SaveAccessoriesAsync(db, draft, existingPlan.Id);
                await AttachmentPersistence.ApplyAsync(db, draft, existingPlan.Id, attachmentResult);
                attachmentResult.DeleteRemovedFiles();
                return null;
            }

            var yearMonth = form.PlanDate.ToString("yyMM");
            var counter = await db.PlanNumberCounters.SingleOrDefaultAsync(x => x.YearMonth == yearMonth);
            if (counter is null)
            {
                counter = new PlanNumberCounter { YearMonth = yearMonth, LastSequence = 0 };
                db.PlanNumberCounters.Add(counter);
            }
            counter.LastSequence++;

            var newPlan = new RiggingPlan
            {
                Id = Guid.NewGuid(),
                PlanNumber = $"{yearMonth}{counter.LastSequence:000}",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Client = client,
                Rigger = rigger
            };
            PlanFormMapper.ApplyToSavedPlan(form, newPlan, client, equipment, rigger, formatCnpj: false);
            CopyCapacityFields(draft.Plan, newPlan);
            ApplyReferences(newPlan, state.EquipmentOptions, state.RiggerOptions);
            db.RiggingPlans.Add(newPlan);

            await db.SaveChangesAsync();
            await SaveAccessoriesAsync(db, draft, newPlan.Id);
            await AttachmentPersistence.ApplyAsync(db, draft, newPlan.Id, attachmentResult);
            attachmentResult.DeleteRemovedFiles();
            draft.PlanId = newPlan.Id;
            return null;
        }
        catch
        {
            attachmentResult.DeleteCreatedFiles();
            throw;
        }
    }

    private static async Task<Client?> ResolveClientAsync(AppDbContext db, PlanFormModel form)
    {
        if (form.ClientId is not null)
        {
            return await db.Clients.SingleOrDefaultAsync(x => x.Id == form.ClientId);
        }

        if (string.IsNullOrWhiteSpace(form.ClientName))
        {
            return null;
        }

        var cnpj = CnpjFormatter.Format(form.ClientCnpj);
        if (cnpj is null || CnpjFormatter.Normalize(cnpj)?.Length != 14)
        {
            throw new InvalidOperationException("Informe um CNPJ válido no formato 00.000.000/0000-00 ou 00.000.000/E08G-12.");
        }

        if ((await db.Clients.AsNoTracking().Select(x => x.Cnpj).ToListAsync())
            .Any(value => CnpjFormatter.Normalize(value) == CnpjFormatter.Normalize(cnpj)))
        {
            throw new InvalidOperationException("Já existe um cliente cadastrado com este CNPJ. Selecione-o na lista.");
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
        return client;
    }

    private static async Task<Equipment?> ResolveEquipmentAsync(AppDbContext db, PlanFormModel form,
        PlanEntryState state)
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
        state.EquipmentOptions.Add(equipment);
        return equipment;
    }

    private static async Task<Rigger?> ResolveRiggerAsync(AppDbContext db, PlanFormModel form, PlanEntryState state)
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
            FullName = string.IsNullOrWhiteSpace(form.RiggerFullName) ? form.RiggerDisplayName.Trim() : form.RiggerFullName.Trim(),
            DisplayName = form.RiggerDisplayName.Trim(),
            RppNumber = string.IsNullOrWhiteSpace(form.RiggerRppNumber) ? "000" : form.RiggerRppNumber.Trim(),
            CreaNumber = string.IsNullOrWhiteSpace(form.RiggerCreaNumber) ? null : form.RiggerCreaNumber.Trim(),
            IsActive = true
        };
        db.Riggers.Add(rigger);
        form.RiggerId = rigger.Id;
        state.RiggerOptions.Add(rigger);
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

    private static async Task SaveAccessoriesAsync(AppDbContext db, PlanDraftState draft, Guid planId)
    {
        var storedItems = await db.PlanAccessories.Where(x => x.PlanId == planId).ToListAsync();
        if (storedItems.Count > 0) db.PlanAccessories.RemoveRange(storedItems);
        foreach (var item in draft.Accessories)
        {
            item.PlanId = planId;
            item.Plan = null!;
            db.PlanAccessories.Add(item);
        }
        if (storedItems.Count > 0 || draft.Accessories.Count > 0) await db.SaveChangesAsync();
    }

    private static void CopyCapacityFields(RiggingPlan? source, RiggingPlan target)
    {
        if (source is null) return;
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
}
