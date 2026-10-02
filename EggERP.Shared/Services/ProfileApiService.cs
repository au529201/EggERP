using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services;

public interface IProfileApiService
{
    Task<UserProfileDto?> GetProfileAsync();

    Task<string?> UpdateProfileAsync(
        UpdateMyProfileRequest request);

    Task<string?> ChangePasswordAsync(
        ChangeMyPasswordRequest request);
}

public class ProfileApiService : IProfileApiService
{
    private readonly HttpClient _http;

    public ProfileApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<UserProfileDto?> GetProfileAsync()
    {
        var response = await _http.GetAsync("api/profile");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<UserProfileDto>();
    }

    public async Task<string?> UpdateProfileAsync(
        UpdateMyProfileRequest request)
    {
        var response = await _http.PutAsJsonAsync(
            "api/profile",
            request);

        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadErrorAsync(response);
    }

    public async Task<string?> ChangePasswordAsync(
        ChangeMyPasswordRequest request)
    {
        var response = await _http.PostAsJsonAsync(
            "api/profile/change-password",
            request);

        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadErrorAsync(response);
    }

    private static async Task<string> ReadErrorAsync(
        HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(text))
        {
            return "The request could not be completed.";
        }

        return text.Trim().Trim('"');
    }
}