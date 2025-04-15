using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ScheduleDay.Components;
using ScheduleDay.Data;
using ScheduleDay.Models;
using System.Text;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents()
	.AddInteractiveWebAssemblyComponents();

// JWT Config
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

if (jwtSettings == null)
{
	throw new InvalidOperationException("JWT settings are not configured properly.");
}

// Authentication
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // needed for google redirect
})

.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtSettings.Issuer,
		ValidAudience = jwtSettings.Audience,
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.ASCII.GetBytes(jwtSettings.SecretKey))
	};
})

.AddCookie(options =>
{
	options.Cookie.SameSite = SameSiteMode.None; // Important for cross-site
	options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
	options.LoginPath = "/api/auth/externallogin";
})

.AddGoogle(options =>
{
	var googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
	options.ClientId = googleAuthNSection["ClientId"] ?? "";
	options.ClientSecret = googleAuthNSection["ClientSecret"] ?? "";
	options.CallbackPath = "/signin-google"; // by default
	options.Scope.Add("https://www.googleapis.com/auth/calendar.readonly");
	options.SaveTokens = true;
});

// DBContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"));
});

// CORS Config
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.WithOrigins(
				"https://localhost:5001",
				"http://localhost:5000",
				"https://localhost:7073",
				"http://localhost:5176",
				"https://localhost:44340",
				"https://scheduledayteamtwo-h9d3gcdcc0d8ecdq.canadacentral-01.azurewebsites.net",
				"https://scheduledayapp-dwhbhsbzecbgcdfy.canadacentral-01.azurewebsites.net",
				"https://scheduledayapp-api-avc2a0acabeadth4.canadacentral-01.azurewebsites.net",
				"https://scheduledayapp-client-a7cqf2g2hncmeggs.canadacentral-01.azurewebsites.net"
			)
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials()
			.WithExposedHeaders("*");
	});
});

// Controllers and services
builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
	});
builder.Services.AddHttpContextAccessor();

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient();

var app = builder.Build();

// Middlewares
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
else
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowAll");
app.UseCookiePolicy(new CookiePolicyOptions
{
	MinimumSameSitePolicy = SameSiteMode.None,
	HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
	Secure = CookieSecurePolicy.Always
});


app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();


app.MapControllers();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode()
	.AddInteractiveWebAssemblyRenderMode()
	.RequireCors("AllowAll")
	.AddAdditionalAssemblies(typeof(ScheduleDay.Client._Imports).Assembly);

app.Run();