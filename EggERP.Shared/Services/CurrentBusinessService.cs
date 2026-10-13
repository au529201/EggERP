
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace EggERP.Shared.Services;

public class CurrentBusinessService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly HttpClient _http;

    public CurrentBusinessService(
        AuthenticationStateProvider authStateProvider,
        HttpClient http)
    {
        _authStateProvider = authStateProvider;
        _http = http;
    }

    public async Task<Guid> GetBusinessIdAsync()
    {
        var authState = await _authStateProvider
            .GetAuthenticationStateAsync();

        var user = authState.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException(
                "You must sign in to access business data.");
        }

        if (user.IsInRole("PlatformAdmin"))
        {
            throw new UnauthorizedAccessException(
                "Platform Administrators cannot access business data.");
        }

        var result = await _http.GetFromJsonAsync<
            CurrentBusinessResponse>("api/current-business");

        if (result is null || result.BusinessId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Unable to identify your business.");
        }

        return result.BusinessId;
    }

    private sealed class CurrentBusinessResponse
    {
        public Guid BusinessId { get; set; }
    }
}
