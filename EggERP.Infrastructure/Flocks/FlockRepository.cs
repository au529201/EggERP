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
            .Where(f => f.BusinessId == businessId && f.CloseDate == null)
            .OrderByDescending(f => f.PlacementDate)
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

    public async Task<FlockMovement> AddFlockMovementAsync(FlockMovement movement)
    {
        _dbContext.FlockMovements.Add(movement);
        await _dbContext.SaveChangesAsync();
        return movement;
    }

    public async Task<int> GetFlockPopulationAsync(Guid flockId)
    {
        var inSum = await _dbContext.FlockMovements
            .Where(m => m.FlockId == flockId && m.Direction == "In")
            .SumAsync(m => (int?)m.Quantity) ?? 0;

        var outSum = await _dbContext.FlockMovements
            .Where(m => m.FlockId == flockId && m.Direction == "Out")
            .SumAsync(m => (int?)m.Quantity) ?? 0;

        return inSum - outSum;
    }

    public async Task<EggMovement> AddEggMovementAsync(EggMovement movement)
    {
        _dbContext.EggMovements.Add(movement);
        await _dbContext.SaveChangesAsync();
        return movement;
    }

    public async Task<int> GetProductEggStockAsync(Guid businessId, Guid productId)
    {
        var inSum = await _dbContext.EggMovements
            .Where(m => m.BusinessId == businessId && m.ProductId == productId && m.Direction == "In")
            .SumAsync(m => (int?)m.Quantity) ?? 0;

        var outSum = await _dbContext.EggMovements
            .Where(m => m.BusinessId == businessId && m.ProductId == productId && m.Direction == "Out")
            .SumAsync(m => (int?)m.Quantity) ?? 0;

        return inSum - outSum;
    }
}