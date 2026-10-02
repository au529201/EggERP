namespace EggERP.Application.Businesses;

public interface IBusinessProvisioningService
{
    Task<(bool Succeeded, Guid? BusinessId, string? Error)>
        CreateBusinessWithOwnerAsync(CreateBusinessOwnerRequest request);
}