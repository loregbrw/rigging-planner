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

    public async Task<IReadOnlyList<Equipment>> GetEquipmentsAsync(bool activeOnly = false)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Equipments.AsNoTracking()
            .Where(x => !activeOnly || x.IsActive)
            .OrderBy(x => x.CraneModel)
            .ToListAsync();
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
}
