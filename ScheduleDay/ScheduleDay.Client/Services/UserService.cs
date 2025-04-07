using System.Net.Http.Json;
using ScheduleDay.Shared.Models;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace ScheduleDay.Client.Services
{
    public class UserService
    {
        private readonly HttpClient _http;
        private readonly ILogger<UserService> _logger;

        public UserService(HttpClient http, ILogger<UserService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<User?> GetUserById(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<User>($"api/users/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user {id}");
                return null;
            }
        }


        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<bool> UpdateUser(int id, User user, string? newPassword = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(newPassword))
                {
                    user.Password = HashPassword(newPassword);
                }

                var response = await _http.PutAsJsonAsync($"api/users/{user.ID}", user);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error updating user: {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user {user.ID}");
                throw;
            }
        }
    }
}