using System.Net.Http.Headers;

namespace ScheduleDay.Client.Services
{
    public static class HttpClientExtensions
    {
        public static HttpClient EnableCookies(this HttpClient client)
        {
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            return client;
        }
    }
}