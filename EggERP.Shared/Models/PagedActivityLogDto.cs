
namespace EggERP.Shared.Models;

public class PagedActivityLogDto
{
    public List<ActivityLogDto> Items { get; set; } = new();

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
