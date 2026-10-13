
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
}

public interface IPlatformBusinessApiService
{
    Task<List<PlatformBusinessDto>> GetBusinessesAsync();

    Task<string?> CreateBusinessAsync(
        CreatePlatformBusinessRequest request);
}

public class PlatformBusinessApiService
    : IPlatformBusinessApiService
{
    private readonly HttpClient _http;

    public PlatformBusinessApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PlatformBusinessDto>>
        GetBusinessesAsync()
    {
        return await _http.GetFromJsonAsync<
            List<PlatformBusinessDto>>(
                "api/provisioning/businesses")
            ?? new List<PlatformBusinessDto>();
    }

    public async Task<string?> CreateBusinessAsync(
        CreatePlatformBusinessRequest request)
    {
        var response = await _http.PostAsJsonAsync(
            "api/provisioning/business", request);

        if (response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsStringAsync();
    }
}
