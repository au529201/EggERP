using EggERP.Domain.Entities;

namespace EggERP.Application.Flocks;

public interface IFlockRepository
{
    Task<List<Flock>> GetActiveByBusinessIdAsync(Guid businessId);
    Task<Flock?> GetByIdAsync(Guid businessId, Guid id);
    Task<Flock?> GetByLinkedProductIdAsync(Guid businessId, Guid productId);
    Task<Flock> AddAsync(Flock flock);

    Task<FlockMovement> AddFlockMovementAsync(FlockMovement movement);
    Task<int> GetFlockPopulationAsync(Guid flockId);

    Task<EggMovement> AddEggMovementAsync(EggMovement movement);
    Task<int> GetProductEggStockAsync(Guid businessId, Guid productId);
}