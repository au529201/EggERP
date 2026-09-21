namespace EggERP.Application.Flocks;

public class FlockDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public DateTime PlacementDate { get; set; }
    public DateTime? CloseDate { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public string? Notes { get; set; }
    public Guid? LinkedProductId { get; set; }
    public string? LinkedProductName { get; set; }
    public int CurrentPopulation { get; set; }
}

public class CreateFlockRequest
{
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = "Chicken";
    public string? Breed { get; set; }
    public DateTime PlacementDate { get; set; } = DateTime.Today;
    public decimal? AcquisitionCost { get; set; }
    public string? Notes { get; set; }
    public Guid? LinkedProductId { get; set; }

    // The flock's starting population is recorded as its first FlockMovement,
    // not a stored count — population stays fully computed from day one.
    public int InitialQuantity { get; set; }
    public string InitialSource { get; set; } = "Bought"; // Bought, Given, Hatched
}

public class RecordFlockMovementRequest
{
    public Guid BusinessId { get; set; }
    public Guid FlockId { get; set; }
    public string Direction { get; set; } = "Out";
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime MovementDate { get; set; }
    public string? Notes { get; set; }
}

public class RecordEggMovementRequest
{
    public Guid BusinessId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? FlockId { get; set; }
    public string Direction { get; set; } = "In";
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime MovementDate { get; set; }
    public string? Notes { get; set; }
}

public interface IFlockService
{
    Task<List<FlockDto>> GetFlocksForBusinessAsync(Guid businessId);
    Task<(bool Succeeded, string? Error)> CreateFlockAsync(CreateFlockRequest request);
    Task<(bool Succeeded, string? Error)> RecordMovementAsync(RecordFlockMovementRequest request);
    Task<(bool Succeeded, string? Error)> RecordEggMovementAsync(RecordEggMovementRequest request);
}