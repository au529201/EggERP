
using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services;

public interface IInventoryApiService
{
    Task<List<InventoryDto>> GetInventoryAsync(Guid businessId);

    Task<InventoryDto> AdjustQuantityAsync(
        Guid businessId, Guid productId, decimal delta);

    Task<List<InventoryBoardRowDto>>
        GetBoardAsOfDateAsync(DateTime asOfDate);

    Task SetThresholdsAsync(
        Guid productId,
        decimal lowThreshold,
        decimal? highThreshold);
}

public class InventoryApiService : IInventoryApiService
{
    private readonly HttpClient _http;

    public InventoryApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<InventoryDto>>
        GetInventoryAsync(Guid businessId)
    {
        return await _http.GetFromJsonAsync<List<InventoryDto>>(
            $"api/inventory/{businessId}") ?? new();
    }

    public async Task<InventoryDto> AdjustQuantityAsync(
        Guid businessId, Guid productId, decimal delta)
    {
        var response = await _http.PostAsJsonAsync(
            $"api/inventory/{businessId}/product/{productId}/adjust",
            new { delta });

        response.EnsureSuccessStatusCode();

        return (await response.Content
            .ReadFromJsonAsync<InventoryDto>())!;
    }

    public async Task<List<InventoryBoardRowDto>>
        GetBoardAsOfDateAsync(DateTime asOfDate)
    {
        var date = asOfDate.ToString("yyyy-MM-dd");

        return await _http
            .GetFromJsonAsync<List<InventoryBoardRowDto>>(
                $"api/inventory/board?asOfDate={date}") ?? new();
    }

    public async Task SetThresholdsAsync(
        Guid productId,
        decimal lowThreshold,
        decimal? highThreshold)
    {
        var response = await _http.PutAsJsonAsync(
            $"api/inventory/product/{productId}/thresholds",
            new
            {
                LowThreshold = lowThreshold,
                HighThreshold = highThreshold
            });

        response.EnsureSuccessStatusCode();
    }
}
