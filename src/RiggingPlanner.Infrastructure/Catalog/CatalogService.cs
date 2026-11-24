using Microsoft.EntityFrameworkCore;
using RiggingPlanner.Application.Catalog;
using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.Infrastructure.Catalog;

public sealed class CatalogService(IDbContextFactory<AppDbContext> dbFactory) : ICatalogService
{
    public async Task<IReadOnlyList<Client>> GetClientsAsync(bool activeOnly = false)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Clients.AsNoTracking()
            .Where(x => !activeOnly || x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Equipment>> GetEquipmentsAsync(bool activeOnly = false, bool includeReports = false)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var query = db.Equipments.AsNoTracking()
            .Where(x => !activeOnly || x.IsActive);
        if (includeReports)
        {
            query = query.Include(x => x.Reports);
        }
        return await query.OrderBy(x => x.CraneModel).ToListAsync();
    }

    public async Task<IReadOnlyList<Rigger>> GetRiggersAsync(bool activeOnly = false)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Riggers.AsNoTracking()
            .Where(x => !activeOnly || x.IsActive)
            .OrderBy(x => x.DisplayName)
            .ToListAsync();
    }

    public async Task<Client?> FindClientAsync(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Clients.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Equipment?> FindEquipmentAsync(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Equipments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Rigger?> FindRiggerAsync(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Riggers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<CatalogSaveResult> SaveClientAsync(ClientSaveRequest request)
    {
        var name = request.Name.Trim();
        var cnpj = CnpjFormatter.Format(request.Cnpj);
        if (cnpj is null || CnpjFormatter.Normalize(cnpj)?.Length != 14)
        {
            return new CatalogSaveResult(CatalogMessages.InvalidCnpj, false);
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        var duplicate = (await db.Clients.AsNoTracking()
                .Where(x => x.Id != request.Id)
                .Select(x => x.Cnpj)
                .ToListAsync())
            .Any(value => CnpjFormatter.Normalize(value) == CnpjFormatter.Normalize(cnpj));
        if (duplicate)
        {
            return new CatalogSaveResult(CatalogMessages.DuplicateCnpj, false);
        }

        var stored = request.Id is null ? null : await db.Clients.FindAsync(request.Id.Value);
        if (request.Id is not null && stored is null)
        {
            return new CatalogSaveResult(CatalogMessages.ClientNotFound, false);
        }

        var created = stored is null;
        var now = DateTime.Now;
        if (stored is null)
        {
            db.Clients.Add(new Client
            {
                Id = Guid.NewGuid(),
                Name = name,
                Cnpj = cnpj,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            stored.Name = name;
            stored.Cnpj = cnpj;
            stored.UpdatedAt = now;
        }

        await db.SaveChangesAsync();
        return new CatalogSaveResult(null, created);
    }

    public async Task<EquipmentSaveResult> SaveEquipmentAsync(EquipmentSaveRequest request)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var stored = request.Id is null
            ? null
            : await db.Equipments.Include(x => x.Reports).SingleOrDefaultAsync(x => x.Id == request.Id.Value);
        if (request.Id is not null && stored is null)
        {
            return new EquipmentSaveResult(CatalogMessages.EquipmentNotFound, null, false);
        }

        var created = stored is null;
        var now = DateTime.Now;
        stored ??= new Equipment { Id = Guid.NewGuid(), IsActive = true, CreatedAt = now };
        stored.EquipmentType = "munck";
        stored.TruckModel = request.TruckModel.Trim();
        stored.LicensePlate = request.LicensePlate.Trim().ToUpperInvariant();
        stored.ChassisNumber = request.ChassisNumber.Trim();
        stored.ManufactureYear = request.ManufactureYear;
        stored.CraneBrand = string.Empty;
        stored.CraneModel = request.CraneModel.Trim();
        stored.UpdatedAt = now;

        if (created)
        {
            db.Equipments.Add(stored);
        }

        await db.SaveChangesAsync();
        return new EquipmentSaveResult(null, stored.Id, created);
    }

    public async Task<CatalogSaveResult> SaveRiggerAsync(RiggerSaveRequest request)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var stored = request.Id is null ? null : await db.Riggers.FindAsync(request.Id.Value);
        if (request.Id is not null && stored is null)
        {
            return new CatalogSaveResult(CatalogMessages.RiggerNotFound, false);
        }

        var created = stored is null;
        var fullName = request.FullName.Trim();
        var displayName = request.DisplayName.Trim();
        var rppNumber = request.RppNumber.Trim();
        var creaNumber = string.IsNullOrWhiteSpace(request.CreaNumber) ? null : request.CreaNumber.Trim();

        if (stored is null)
        {
            db.Riggers.Add(new Rigger
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                DisplayName = displayName,
                RppNumber = rppNumber,
                CreaNumber = creaNumber,
                IsActive = true
            });
        }
        else
        {
            stored.FullName = fullName;
            stored.DisplayName = displayName;
            stored.RppNumber = rppNumber;
            stored.CreaNumber = creaNumber;
        }

        await db.SaveChangesAsync();
        return new CatalogSaveResult(null, created);
    }

    public async Task ToggleClientStatusAsync(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var stored = await db.Clients.FindAsync(id);
        if (stored is null) return;
        stored.IsActive = !stored.IsActive;
        stored.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task ToggleEquipmentStatusAsync(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var stored = await db.Equipments.FindAsync(id);
        if (stored is null) return;
        stored.IsActive = !stored.IsActive;
        stored.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task ToggleRiggerStatusAsync(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var stored = await db.Riggers.FindAsync(id);
        if (stored is null) return;
        stored.IsActive = !stored.IsActive;
        await db.SaveChangesAsync();
    }

    public async Task<string?> DeleteClientAsync(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var client = await db.Clients.FindAsync(id);
        if (client is null) return null;
        if (await db.RiggingPlans.AnyAsync(x => x.ClientId == client.Id && x.DeletedAt == null && x.Status != "finalized"))
        {
            return CatalogMessages.ClientInUse;
        }

        db.Clients.Remove(client);
        await db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> DeleteEquipmentAsync(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var equipment = await db.Equipments.FindAsync(id);
        if (equipment is null) return null;
        if (await db.RiggingPlans.AnyAsync(x => x.EquipmentId == equipment.Id && x.DeletedAt == null && x.Status != "finalized"))
        {
            return CatalogMessages.EquipmentInUse;
        }

        db.Equipments.Remove(equipment);
        await db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> DeleteRiggerAsync(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var rigger = await db.Riggers.FindAsync(id);
        if (rigger is null) return null;
        if (await db.RiggingPlans.AnyAsync(x => x.RiggerId == rigger.Id && x.DeletedAt == null && x.Status != "finalized"))
        {
            return CatalogMessages.RiggerInUse;
        }

        db.Riggers.Remove(rigger);
        await db.SaveChangesAsync();
        return null;
    }
}
