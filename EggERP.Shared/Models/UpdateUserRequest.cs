namespace EggERP.Shared.Models
{
    public class UpdateUserRequest
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Staff";
    }
}