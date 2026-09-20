using EggERP.Application.Flocks;
using EggERP.Application.Inventory;
using EggERP.Application.Products;
using EggERP.Domain.Entities;

namespace EggERP.Infrastructure.Flocks;

public class FlockService : IFlockService
{
    private readonly IFlockRepository _flockRepository;
    private readonly IInventoryService _inventoryService;
    private readonly IProductRepository _productRepository;

    public FlockService(
        IFlockRepository flockRepository,
        IInventoryService inventoryService,
        IProductRepository productRepository)
    {
        _flockRepository = flockRepository;
        _inventoryService = inventoryService;
        _productRepository = productRepository;
    }

    public async Task<List<FlockDto>> GetFlocksForBusinessAsync(Guid businessId)
    {
        var flocks = await _flockRepository.GetActiveByBusinessIdAsync(businessId);
        var dtos = new List<FlockDto>();

        foreach (var f in flocks)
        {
            string? productName = null;
            if (f.LinkedProductId.HasValue)
            {
                var product = await _productRepository.GetByIdAsync(businessId, f.LinkedProductId.Value);
                productName = product?.Name;
            }

            dtos.Add(new FlockDto
            {
                Id = f.Id,
                BirdType = f.BirdType,
                Source = f.Source,
                AcquisitionDate = f.AcquisitionDate,
                InitialCount = f.InitialCount,
                CurrentCount = f.CurrentCount,
                AcquisitionCost = f.AcquisitionCost,
                Notes = f.Notes,
                IsActive = f.IsActive,
                LinkedProductId = f.LinkedProductId,
                LinkedProductName = productName
            });
        }

        return dtos;
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

        if (request.LinkedProductId.HasValue)
        {
            var product = await _productRepository.GetByIdAsync(request.BusinessId, request.LinkedProductId.Value);
            if (product is null)
            {
                return (false, "Linked product not found in this business.");
            }
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
            LinkedProductId = request.LinkedProductId,
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

    public async Task<(bool Succeeded, string? Error)> RecordEggProductionAsync(RecordEggProductionRequest request)
    {
        if (request.QuantityProduced <= 0)
        {
            return (false, "Quantity produced must be greater than zero.");
        }

        var flock = await _flockRepository.GetByIdAsync(request.BusinessId, request.FlockId);
        if (flock is null)
        {
            return (false, "Flock not found in this business.");
        }

        if (!flock.LinkedProductId.HasValue)
        {
            return (false, "This flock has no linked product. Edit the flock to link it to an egg product before recording production.");
        }

        var production = new EggProduction
        {
            Id = Guid.NewGuid(),
            FlockId = flock.Id,
            ProductionDate = request.ProductionDate,
            QuantityProduced = request.QuantityProduced,
            Notes = request.Notes,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _flockRepository.AddEggProductionAsync(production);

        // Egg production adds to the linked product's sellable inventory.
        await _inventoryService.AdjustQuantityAsync(
            request.BusinessId, flock.LinkedProductId.Value, request.QuantityProduced);

        return (true, null);
    }
}