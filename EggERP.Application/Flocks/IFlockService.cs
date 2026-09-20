namespace EggERP.Application.Flocks;

public class FlockDto
{
    public Guid Id { get; set; }
    public string BirdType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime AcquisitionDate { get; set; }
    public int InitialCount { get; set; }
    public int CurrentCount { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public Guid? LinkedProductId { get; set; }
    public string? LinkedProductName { get; set; }
}

public class CreateFlockRequest
{
    public Guid BusinessId { get; set; }
    public string BirdType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime AcquisitionDate { get; set; }
    public int InitialCount { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public string? Notes { get; set; }
    public Guid? LinkedProductId { get; set; }
}

public class RecordFlockMovementRequest
{
    public Guid BusinessId { get; set; }
    public Guid FlockId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime MovementDate { get; set; }
    public string? Notes { get; set; }
}

public class RecordEggProductionRequest
{
    public Guid BusinessId { get; set; }
    public Guid FlockId { get; set; }
    public DateTime ProductionDate { get; set; }
    public int QuantityProduced { get; set; }
    public string? Notes { get; set; }
}

public interface IFlockService
{
    Task<List<FlockDto>> GetFlocksForBusinessAsync(Guid businessId);
    Task<(bool Succeeded, string? Error)> CreateFlockAsync(CreateFlockRequest request);
    Task<(bool Succeeded, string? Error)> RecordMovementAsync(RecordFlockMovementRequest request);
    Task<(bool Succeeded, string? Error)> RecordEggProductionAsync(RecordEggProductionRequest request);
}