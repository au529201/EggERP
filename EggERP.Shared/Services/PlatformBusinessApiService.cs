
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace EggERP.Shared.Services;

public class CreatePlatformBusinessRequest
{
    [Required]
    [MaxLength(200)]
    public string BusinessName { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string OwnerFullName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string OwnerEmail { get; set; } = "";

    [EmailAddress]
    public string? BusinessEmail { get; set; }

    public string? Phone { get; set; }
    public string? City { get; set; }
}

public class PlatformBusinessDto
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public string OwnerEmail { get; set; } = "";
    public bool OwnerEmailConfirmed { get; set; }
    public bool IsActive { get; set; }
}

public interface IPlatformBusinessApiService
{
    Task<List<PlatformBusinessDto>> GetBusinessesAsync();

    Task<string?> CreateBusinessAsync(
        CreatePlatformBusinessRequest request);

    Task<string?> ChangeBusinessStatusAsync(
        Guid businessId,
        bool isActive);
}

public class PlatformBusinessApiService : IPlatformBusinessApiService
{
    private readonly HttpClient _http;

    public PlatformBusinessApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PlatformBusinessDto>> GetBusinessesAsync()
    {
        using var response = await _http.GetAsync(
            "api/provisioning/businesses");

        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"Unable to load businesses. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.ReasonPhrase}). " +
                $"Details: {details[..Math.Min(details.Length, 300)]}");
        }

        return await response.Content.ReadFromJsonAsync<
            List<PlatformBusinessDto>>() ?? new();
    }

    public async Task<string?> CreateBusinessAsync(
        CreatePlatformBusinessRequest request)
    {
        using var response = await _http.PostAsJsonAsync(
            "api/provisioning/business",
            request);

        if (response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string?> ChangeBusinessStatusAsync(
        Guid businessId,
        bool isActive)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/provisioning/businesses/{businessId}/status");

        request.Headers.Add(
            "X-EzEggERP-Admin-Action",
            "BusinessStatusChange");

        request.Content = JsonContent.Create(new { isActive });

        using var response = await _http.SendAsync(request);

        if (response.IsSuccessStatusCode)
            return null;

        return $"Unable to update business status. HTTP {(int)response.StatusCode}.";
    }
}
