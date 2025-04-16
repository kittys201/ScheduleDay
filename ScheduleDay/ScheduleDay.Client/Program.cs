using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using ScheduleDay.Client.Providers;
using ScheduleDay.Client.Services;
using ScheduleDay.Client.Handlers;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<ScheduleDay.Client.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configuración de servicios
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<ActivityMonitor>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<UserService>();


// Configure HttpClient with JWT interceptor
builder.Services.AddScoped<CustomAuthorizationMessageHandler>();

// Configure HttpClient with correct URL base, It is important to mention that every time you create a virtual machine you must change the base URL and so that they can work locally, keep localhost
var baseAddress = builder.HostEnvironment.IsProduction()
    ? "https://scheduledayapp-api-avc2a0acabeadth4.canadacentral-01.azurewebsites.net"
    : "https://localhost:7073/";

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(baseAddress);
})
.AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

// HttpClient by default
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("API"));

await builder.Build().RunAsync();