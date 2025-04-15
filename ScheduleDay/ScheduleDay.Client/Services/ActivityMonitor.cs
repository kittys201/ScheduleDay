using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ScheduleDay.Client.Services
{
    public class ActivityMonitor : IDisposable
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigationManager;
        private readonly HttpClient _httpClient;
        private Timer? _timer;
        private DateTime _lastActivity;
        private bool _isDisposed;

        public ActivityMonitor(
            AuthenticationStateProvider authStateProvider,
            NavigationManager navigationManager,
            HttpClient httpClient)
        {
            _authStateProvider = authStateProvider;
            _navigationManager = navigationManager;
            _httpClient = httpClient;
            _lastActivity = DateTime.Now;
            StartMonitoring();
        }

        private void StartMonitoring()
        {
            _timer = new Timer(CheckActivity, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        private async void CheckActivity(object? state)
        {
            if (_isDisposed) return;

            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                var timeSinceLastActivity = DateTime.Now - _lastActivity;
                if (timeSinceLastActivity.TotalMinutes >= 14) // Refresh before expiration (15 min)
                {
                    try
                    {
                        var response = await _httpClient.PostAsync("api/auth/refresh", null);
                        if (!response.IsSuccessStatusCode)
                        {
                            await HandleSessionExpiration();
                        }
                        else
                        {
                            _lastActivity = DateTime.Now;
                        }
                    }
                    catch
                    {
                        await HandleSessionExpiration();
                    }
                }
            }
        }

        private async Task HandleSessionExpiration()
        {
            try
            {
                await _httpClient.PostAsync("api/auth/logout", null);
            }
            finally
            {
                _navigationManager.NavigateTo("/login?expired=true", true);
            }
        }

        public void UpdateActivity()
        {
            if (!_isDisposed)
            {
                _lastActivity = DateTime.Now;
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _timer?.Dispose();
            }
        }

        // Check if session is Active.
        public bool IsSessionActive()
        {
            return (DateTime.Now - _lastActivity).TotalMinutes < 15;
        }

        // Reset timer manually
        public void ResetTimer()
        {
            if (!_isDisposed)
            {
                _timer?.Change(TimeSpan.Zero, TimeSpan.FromMinutes(1));
                UpdateActivity();
            }
        }
    }
}