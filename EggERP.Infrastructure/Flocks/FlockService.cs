using EggERP.Application.Flocks;
using EggERP.Domain.Entities;

namespace EggERP.Infrastructure.Flocks;

public class FlockService : IFlockService
{
    private readonly IFlockRepository _flockRepository;

    public FlockService(IFlockRepository flockRepository)
    {
        _flockRepository = flockRepository;
    }

    public async Task<List<FlockDto>> GetFlocksForBusinessAsync(Guid businessId)
    {
        var flocks = await _flockRepository.GetActiveByBusinessIdAsync(businessId);

        return flocks.Select(f => new FlockDto
        {
            Id = f.Id,
            BirdType = f.BirdType,
            Source = f.Source,
            AcquisitionDate = f.AcquisitionDate,
            InitialCount = f.InitialCount,
            CurrentCount = f.CurrentCount,
            AcquisitionCost = f.AcquisitionCost,
            Notes = f.Notes,
            IsActive = f.IsActive
        }).ToList();
    }

    public async Task<(bool Succeeded, string? Error)> CreateFlockAsync(CreateFlockRequest request)
    {
        if (!FlockOptions.BirdTypes.Contains(request.BirdType))
        {
            return (false, "Invalid bird type.");
        }

        if (!FlockOptions.Sources.Contains(request.Source))
        {
            return (false, "Invalid source.");
        }

        if (request.InitialCount <= 0)
        {
            return (false, "Initial count must be greater than zero.");
        }

        var flock = new Flock
        {
            Id = Guid.NewGuid(),
            BusinessId = request.BusinessId,
            BirdType = request.BirdType,
            Source = request.Source,
            AcquisitionDate = request.AcquisitionDate,
            InitialCount = request.InitialCount,
            CurrentCount = request.InitialCount,
            AcquisitionCost = request.AcquisitionCost,
            Notes = request.Notes,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _flockRepository.AddAsync(flock);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> RecordMovementAsync(RecordFlockMovementRequest request)
    {
        if (!FlockOptions.MovementReasons.Contains(request.Reason))
        {
            return (false, "Invalid movement reason.");
        }

        if (request.Quantity <= 0)
        {
            return (false, "Quantity must be greater than zero.");
        }

        var flock = await _flockRepository.GetByIdAsync(request.BusinessId, request.FlockId);
        if (flock is null)
        {
            return (false, "Flock not found in this business.");
        }

        if (request.Quantity > flock.CurrentCount)
        {
            return (false, $"Quantity exceeds current count ({flock.CurrentCount}).");
        }

        flock.CurrentCount -= request.Quantity;
        flock.UpdatedAtUtc = DateTime.UtcNow;
        await _flockRepository.UpdateAsync(flock);

        var movement = new FlockMovement
        {
            Id = Guid.NewGuid(),
            FlockId = flock.Id,
            Reason = request.Reason,
            Quantity = request.Quantity,
            MovementDate = request.MovementDate,
            Notes = request.Notes,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _flockRepository.AddMovementAsync(movement);
        return (true, null);
    }
}