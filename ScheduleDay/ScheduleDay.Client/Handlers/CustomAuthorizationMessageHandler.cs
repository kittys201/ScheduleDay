using Blazored.LocalStorage;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace ScheduleDay.Client.Handlers
{
    public class CustomAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly ILogger<CustomAuthorizationMessageHandler> _logger;

        public CustomAuthorizationMessageHandler(
            ILocalStorageService localStorage,
            ILogger<CustomAuthorizationMessageHandler> logger)
        {
            _localStorage = localStorage;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try 
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = 
                        new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogInformation($"Added token to request: {request.RequestUri}");
                }
                else 
                {
                    _logger.LogWarning($"No token found for request: {request.RequestUri}");
                }

                return await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in authorization handler");
                throw;
            }
        }
    }
}