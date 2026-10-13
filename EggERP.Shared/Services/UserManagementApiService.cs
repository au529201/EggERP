
using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services
{
    public interface IUserManagementApiService
    {
        Task<List<UserDto>> GetUsersAsync();

        Task<string?> CreateUserAsync(CreateUserRequest request);

        Task<string?> UpdateUserAsync(UpdateUserRequest request);

        Task<string?> DeactivateUserAsync(Guid userId);

        Task<string?> ActivateUserAsync(Guid userId);

        Task<string?> ResendInvitationAsync(Guid userId);
    }

    public class UserManagementApiService : IUserManagementApiService
    {
        private readonly HttpClient _http;

        public UserManagementApiService(HttpClient http)
        {
            _http = http;
        }

        // ==========================================
        // GET USERS
        // ==========================================
        public async Task<List<UserDto>> GetUsersAsync()
        {
            var result = await _http.GetFromJsonAsync<List<UserDto>>(
                "api/users");

            return result ?? new List<UserDto>();
        }

        // ==========================================
        // CREATE USER
        // ==========================================
        public async Task<string?> CreateUserAsync(
            CreateUserRequest request)
        {
            var response = await _http.PostAsJsonAsync(
                "api/users",
                request);

            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        // ==========================================
        // UPDATE USER
        // ==========================================
        public async Task<string?> UpdateUserAsync(
            UpdateUserRequest request)
        {
            var response = await _http.PutAsJsonAsync(
                $"api/users/{request.Id}",
                request);

            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        // ==========================================
        // DEACTIVATE USER
        // ==========================================
        public async Task<string?> DeactivateUserAsync(Guid userId)
        {
            var response = await _http.DeleteAsync(
                $"api/users/{userId}");

            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        // ==========================================
        // ACTIVATE USER
        // ==========================================
        public async Task<string?> ActivateUserAsync(Guid userId)
        {
            var response = await _http.PostAsync(
                $"api/users/{userId}/activate",
                null);

            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        // ==========================================
        // RESEND EMPLOYEE INVITATION
        // ==========================================
        public async Task<string?> ResendInvitationAsync(Guid userId)
        {
            var response = await _http.PostAsync(
                $"api/users/{userId}/resend-invitation",
                null);

            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
