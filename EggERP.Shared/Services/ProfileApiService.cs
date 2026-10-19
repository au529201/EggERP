
using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services;

public interface IProfileApiService
{
    Task<UserProfileDto?> GetProfileAsync();

    Task<string?> UpdateProfileAsync(
        UpdateMyProfileRequest request,
        string csrfToken);
    Task<string?> ChangePasswordAsync(
        ChangeMyPasswordRequest request,
        string csrfToken);
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
            return null;

        return await response.Content
            .ReadFromJsonAsync<UserProfileDto>();
    }

    public async Task<string?> UpdateProfileAsync(
        UpdateMyProfileRequest request,
        string csrfToken)
    {
        if (string.IsNullOrWhiteSpace(csrfToken))
            return "An antiforgery token is required.";

        using var message = new HttpRequestMessage(
            HttpMethod.Put,
            "api/profile");

        message.Headers.TryAddWithoutValidation(
            "X-CSRF-TOKEN",
            csrfToken);

        message.Content = JsonContent.Create(request);

        using var response = await _http.SendAsync(message);

        if (response.IsSuccessStatusCode)
            return null;

        return await ReadErrorAsync(response);
    }
    public async Task<string?> ChangePasswordAsync(
        ChangeMyPasswordRequest request,
        string csrfToken)
    {
        if (string.IsNullOrWhiteSpace(csrfToken))
            return "An antiforgery token is required.";

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            "api/profile/change-password");

        message.Headers.TryAddWithoutValidation(
            "X-CSRF-TOKEN",
            csrfToken);

        message.Content = JsonContent.Create(request);

        using var response = await _http.SendAsync(message);

        if (response.IsSuccessStatusCode)
            return null;

        return await ReadErrorAsync(response);
    }

    private static async Task<string> ReadErrorAsync(
        HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(text))
            return "The request could not be completed.";

        return text.Trim().Trim('"');
    }
}
