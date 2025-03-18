using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using Blazored.LocalStorage;

namespace ScheduleDay.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            HttpClient http,
            AuthenticationStateProvider authStateProvider,
            ILocalStorageService localStorage,
            ILogger<AuthService> logger)
        {
            _http = http;
            _http.BaseAddress = new Uri("https://localhost:7073/");
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> Register(string name, string email, string password)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/register", new
                {
                    Name = name,
                    Email = email,
                    Password = password
                });

                _logger.LogInformation($"Register response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
                    return (true, result?.Message ?? "Registro exitoso");
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Register fallido: {error}");
                return (false, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro");
                return (false, $"Error durante el registro: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> Login(string email, string password, bool rememberMe)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login", new
                {
                    Email = email,
                    Password = password,
                    RememberMe = rememberMe
                });

                _logger.LogInformation($"Login response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var loginResult = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (loginResult?.Token != null)
                    {
                        await _localStorage.SetItemAsync("authToken", loginResult.Token);
                        ((Providers.CustomAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
                        return (true, "Login exitoso");
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Login fallido: {errorContent}");
                return (false, "Credenciales inválidas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                return (false, $"Error durante el login: {ex.Message}");
            }
        }

        public async Task<bool> CheckAuthState()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                if (string.IsNullOrEmpty(token))
                    return false;

                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await _http.GetAsync("api/auth/state");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthState>();
                    return result?.IsAuthenticated ?? false;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando estado de autenticación");
                return false;
            }
        }

        public async Task Logout()
        {
            try
            {
                await _localStorage.RemoveItemAsync("authToken");
                _http.DefaultRequestHeaders.Authorization = null;
                ((Providers.CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el logout");
            }
        }

        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public int UserId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        private class RegisterResponse
        {
            public string Message { get; set; } = string.Empty;
        }

        private class AuthState
        {
            public bool IsAuthenticated { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
        }
    }
}