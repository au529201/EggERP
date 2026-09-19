using EggERP.Domain.Entities;

namespace EggERP.Application.Flocks;

public interface IFlockRepository
{
    Task<List<Flock>> GetActiveByBusinessIdAsync(Guid businessId);
    Task<Flock?> GetByIdAsync(Guid businessId, Guid id);
    Task<Flock> AddAsync(Flock flock);
    Task<bool> UpdateAsync(Flock flock);
    Task<FlockMovement> AddMovementAsync(FlockMovement movement);
}