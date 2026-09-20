using EggERP.Application.Flocks;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Infrastructure.Flocks;

public class FlockRepository : IFlockRepository
{
    private readonly EggERPDbContext _dbContext;

    public FlockRepository(EggERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Flock>> GetActiveByBusinessIdAsync(Guid businessId)
    {
        return _dbContext.Flocks
            .Where(f => f.BusinessId == businessId && f.IsActive)
            .OrderByDescending(f => f.AcquisitionDate)
            .ToListAsync();
    }

    public Task<Flock?> GetByIdAsync(Guid businessId, Guid id)
    {
        return _dbContext.Flocks
            .FirstOrDefaultAsync(f => f.BusinessId == businessId && f.Id == id);
    }

    public async Task<Flock> AddAsync(Flock flock)
    {
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();
        return flock;
    }

    public async Task<bool> UpdateAsync(Flock flock)
    {
        _dbContext.Flocks.Update(flock);
        var affected = await _dbContext.SaveChangesAsync();
        return affected > 0;
    }

    public async Task<FlockMovement> AddMovementAsync(FlockMovement movement)
    {
        _dbContext.FlockMovements.Add(movement);
        await _dbContext.SaveChangesAsync();
        return movement;
    }

    public async Task<EggProduction> AddEggProductionAsync(EggProduction production)
    {
        _dbContext.EggProductions.Add(production);
        await _dbContext.SaveChangesAsync();
        return production;
    }
}