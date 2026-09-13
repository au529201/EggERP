namespace EggERP.Application.Users;

public class BusinessUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateBusinessUserRequest
{
    public Guid BusinessId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Manager" or "Staff"
}

public class UpdateBusinessUserRequest
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public interface IUserManagementService
{
    Task<List<BusinessUserDto>> GetUsersForBusinessAsync(Guid businessId);
    Task<(bool Succeeded, string? Error)> CreateUserAsync(CreateBusinessUserRequest request);
    Task<(bool Succeeded, string? Error)> UpdateUserAsync(UpdateBusinessUserRequest request);
    Task<(bool Succeeded, string? Error)> DeactivateUserAsync(Guid businessId, Guid userId);
}