namespace EggERP.Shared.Models;

public class UserProfileDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Role { get; set; } = string.Empty;

    public Guid BusinessId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

public class UpdateMyProfileRequest
{
    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}

public class ChangeMyPasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;
}