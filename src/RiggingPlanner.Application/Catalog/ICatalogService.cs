using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Application.Catalog;

public interface ICatalogService
{
    Task<IReadOnlyList<Client>> GetClientsAsync(bool activeOnly = false);
    Task<IReadOnlyList<Equipment>> GetEquipmentsAsync(bool activeOnly = false);
    Task<IReadOnlyList<Rigger>> GetRiggersAsync(bool activeOnly = false);
    Task<Client?> FindClientAsync(Guid id);
    Task<Equipment?> FindEquipmentAsync(Guid id);
    Task<Rigger?> FindRiggerAsync(Guid id);
}
