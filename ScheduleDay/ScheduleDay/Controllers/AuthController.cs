using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ScheduleDay.Data;
using ScheduleDay.Shared.Models;
using ScheduleDay.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;


namespace ScheduleDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly JwtSettings _jwtSettings;

        public AuthController(
            AppDbContext context,
            ILogger<AuthController> logger,
            IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Submit login request to the server
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid input data" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == request.Email.ToLower());

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    return Unauthorized(new { message = "Incorrect email or password" });
                }

                var token = GenerateJwtToken(user);

                _logger.LogInformation($"{user.Email} has logged in successfully");

                return Ok(new
                {
                    token = token,
                    userId = user.ID,
                    name = user.Name,
                    email = user.Email,
                    rememberMe = request.RememberMe
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex}");
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Register user in database
            try
            {
                var userExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
                if (userExists)
                {
                    return BadRequest(new { Message = "Email is already registered" });
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    Password = hashedPassword
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "User successfully registered" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en registro: {ex}");
                return StatusCode(500, new { Message = "Internal Server Error" });
            }
        }

        [HttpGet("state")]
        [Authorize]
        public IActionResult GetAuthState()
        {
            // Get authentication state
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var name = User.FindFirst(ClaimTypes.Name)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;

                return Ok(new
                {
                    isAuthenticated = true,
                    userId = userId,
                    name = name,
                    email = email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting authentication state: {ex}");
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }

        [HttpPost("refresh")]
        [Authorize]
        public IActionResult RefreshToken()
        {
            // Refresh JWT token
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var name = User.FindFirst(ClaimTypes.Name)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var user = new User
                {
                    ID = int.Parse(userId),
                    Name = name ?? string.Empty,
                    Email = email ?? string.Empty
                };

                var token = GenerateJwtToken(user);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Refresh token error: {ex}");
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }




// // GOOGLE // // GOOGLE // // GOOGLE // // GOOGLE // // GOOGLE // // GOOGLE // // GOOGLE // // GOOGLE // // GOOGLE
        // GOOGLE // // GOOGLE // // GOOGLE // // GOOGLE // // GOOGLE // // GOOGLE // // GOOGLE // // GOOGLE // // GOOGLE

        [HttpGet("externallogin")]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string returnUrl = null) {
            // Redirect to external login provider
            var redirectUrl = Url.Action("googleCallback", "Auth", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);  
        }

        [HttpGet("googlecallback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback(string returnUrl = null, string remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError))
            {
                return BadRequest($"Error from external provider: {remoteError}");
            }

            // Authenticate the user with Google
            // here we are using the default authentication scheme for Google
    
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return BadRequest("Error authenticating");
            }
            // Get user claims from the result
            // we can use the claims to get the user information
            var claims = result.Principal?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email claim not found");
            }

            // verify if the user exists in the database we aready have
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                // if user does not exist, create a new user
                // we can use the name and email from the claims
                user = new User
                {
                    Name = name ?? email, // in case name is null
                    Email = email,
                    Password = "GoogleAccount" // or any default value, since we are not using it
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Generate JWT token for the user
            var token = GenerateJwtToken(user);

            _logger.LogInformation($"{user.Email} Logged in with Google successfully");


            // return Ok(new { token, userId = user.ID, name = user.Name, email = user.Email });
            var redirectUrl = $"https://scheduledayapp-api-avc2a0acabeadth4.canadacentral-01.azurewebsites.net/auth-complete?token={token}&email={Uri.EscapeDataString(user.Email)}&name={Uri.EscapeDataString(user.Name)}";
            return Redirect(redirectUrl);

        }



        private string GenerateJwtToken(User user)
        {
            // Generate JWT token for authentication
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        
    }
}