namespace EggERP.Shared.Models
{
    public class CreateUserRequest
    {
        public Guid BusinessId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Staff";
    }
}