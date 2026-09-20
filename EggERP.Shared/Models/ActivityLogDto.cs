namespace EggERP.Shared.Models
{
    public class ActivityLogDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}