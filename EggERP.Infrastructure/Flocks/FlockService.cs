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

            var population = await _flockRepository.GetFlockPopulationAsync(f.Id);

            dtos.Add(new FlockDto
            {
                Id = f.Id,
                Name = f.Name,
                Species = f.Species,
                Breed = f.Breed,
                PlacementDate = f.PlacementDate,
                CloseDate = f.CloseDate,
                AcquisitionCost = f.AcquisitionCost,
                Notes = f.Notes,
                LinkedProductId = f.LinkedProductId,
                LinkedProductName = productName,
                CurrentPopulation = population
            });
        }

        return dtos;
    }

    public async Task<(bool Succeeded, string? Error)> CreateFlockAsync(CreateFlockRequest request)
    {
        if (!FlockOptions.Species.Contains(request.Species))
        {
            return (false, "Invalid species.");
        }

        if (!FlockOptions.FlockInReasons.Contains(request.InitialSource))
        {
            return (false, "Invalid initial source.");
        }

        if (request.InitialQuantity <= 0)
        {
            return (false, "Initial quantity must be greater than zero.");
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
            Name = request.Name,
            Species = request.Species,
            Breed = request.Breed,
            PlacementDate = request.PlacementDate,
            AcquisitionCost = request.AcquisitionCost,
            Notes = request.Notes,
            LinkedProductId = request.LinkedProductId,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _flockRepository.AddAsync(flock);

        // Starting population is the flock's first movement, keeping population
        // fully computed rather than duplicated as a separately-stored count.
        await _flockRepository.AddFlockMovementAsync(new FlockMovement
        {
            Id = Guid.NewGuid(),
            FlockId = flock.Id,
            MovementDate = request.PlacementDate,
            Direction = "In",
            Reason = request.InitialSource,
            Quantity = request.InitialQuantity,
            Notes = "Initial population",
            CreatedAtUtc = DateTime.UtcNow
        });

        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> RecordMovementAsync(RecordFlockMovementRequest request)
    {
        if (request.Direction != "In" && request.Direction != "Out")
        {
            return (false, "Direction must be 'In' or 'Out'.");
        }

        var validReasons = request.Direction == "In" ? FlockOptions.FlockInReasons : FlockOptions.FlockOutReasons;
        if (!validReasons.Contains(request.Reason))
        {
            return (false, $"'{request.Reason}' is not a valid reason for {request.Direction} movements.");
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

        if (request.Direction == "Out")
        {
            var population = await _flockRepository.GetFlockPopulationAsync(request.FlockId);
            if (request.Quantity > population)
            {
                return (false, $"Quantity exceeds current population ({population}).");
            }
        }

        await _flockRepository.AddFlockMovementAsync(new FlockMovement
        {
            Id = Guid.NewGuid(),
            FlockId = request.FlockId,
            MovementDate = request.MovementDate,
            Direction = request.Direction,
            Reason = request.Reason,
            Quantity = request.Quantity,
            Notes = request.Notes,
            CreatedAtUtc = DateTime.UtcNow
        });

        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> RecordEggMovementAsync(RecordEggMovementRequest request)
    {
        if (request.Direction != "In" && request.Direction != "Out")
        {
            return (false, "Direction must be 'In' or 'Out'.");
        }

        if (request.Reason == "Sold")
        {
            return (false, "Sold egg movements are recorded automatically through the Sales page.");
        }

        var validReasons = request.Direction == "In" ? FlockOptions.EggInReasons : FlockOptions.EggOutReasons;
        if (!validReasons.Contains(request.Reason))
        {
            return (false, $"'{request.Reason}' is not a valid reason for {request.Direction} egg movements.");
        }

        if (request.Quantity <= 0)
        {
            return (false, "Quantity must be greater than zero.");
        }

        var product = await _productRepository.GetByIdAsync(request.BusinessId, request.ProductId);
        if (product is null)
        {
            return (false, "Product not found in this business.");
        }

        if (request.Reason == "Hatched" && !request.FlockId.HasValue)
        {
            return (false, "A flock must be selected for hatched eggs, since it adds birds to that flock.");
        }

        if (request.Direction == "Out")
        {
            var stock = await _flockRepository.GetProductEggStockAsync(request.BusinessId, request.ProductId);
            if (request.Quantity > stock)
            {
                return (false, $"Quantity exceeds current egg stock ({stock}).");
            }
        }

        await _flockRepository.AddEggMovementAsync(new EggMovement
        {
            Id = Guid.NewGuid(),
            BusinessId = request.BusinessId,
            ProductId = request.ProductId,
            FlockId = request.FlockId,
            MovementDate = request.MovementDate,
            Direction = request.Direction,
            Reason = request.Reason,
            Quantity = request.Quantity,
            Notes = request.Notes,
            CreatedAtUtc = DateTime.UtcNow
        });

        // Keep today's stored Inventory number in sync alongside the new
        // ledger, without yet making the ledger the sole source of truth
        // (that switch is Step 2, when Sale is wired to this same ledger).
        var delta = request.Direction == "In" ? (decimal)request.Quantity : -(decimal)request.Quantity;
        await _inventoryService.AdjustQuantityAsync(request.BusinessId, request.ProductId, delta);

        // Hatched eggs become new birds in the same flock.
        if (request.Reason == "Hatched")
        {
            await _flockRepository.AddFlockMovementAsync(new FlockMovement
            {
                Id = Guid.NewGuid(),
                FlockId = request.FlockId!.Value,
                MovementDate = request.MovementDate,
                Direction = "In",
                Reason = "Hatched",
                Quantity = request.Quantity,
                Notes = "Auto-recorded from hatched egg movement",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        return (true, null);
    }
}