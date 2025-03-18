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
builder.Services.AddScoped<ActivityMonitor>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TaskService>();

// Configuración de HttpClient con interceptor JWT
builder.Services.AddScoped<CustomAuthorizationMessageHandler>();

// Configuración del HttpClient con la URL base correcta
var baseAddress = builder.HostEnvironment.IsProduction() 
    ? "https://scheduledayteamtwo-h9d3gcdcc0d8ecdq.canadacentral-01.azurewebsites.net/"
    : "https://localhost:7073/";

builder.Services.AddHttpClient("API", client => 
{
    client.BaseAddress = new Uri(baseAddress);
})
.AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

// HttpClient por defecto
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("API"));

await builder.Build().RunAsync();