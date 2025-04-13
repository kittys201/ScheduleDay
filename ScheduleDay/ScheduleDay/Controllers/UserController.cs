using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleDay.Data;
using ScheduleDay.Shared.Models;
using System.Security.Claims;


namespace ScheduleDay.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class UsersController : ControllerBase
	{
		private readonly AppDbContext _context;

		public UsersController(AppDbContext context)
		{
			_context = context;
		}


		private int GetUserId()
		{
			return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<User>> GetUser(int id)
		{
			var userId = GetUserId();

			if (id != userId)
			{
				return Forbid(); // El usuario solo puede ver su propia información
			}

			var user = await _context.Users.FindAsync(id);
			if (user == null)
			{
				//???
				return NotFound();
			}
			return user;
		}


		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateUser(int id, User user)
		{
			var userId = GetUserId();

			var provider = User.FindFirst("auth_provider")?.Value;

			if (id != userId || provider == "Google")
			{
				return Forbid();
			}

			var existingUser = await _context.Users.FindAsync(id);
			if (existingUser == null)
			{
				return NotFound();
			}

			existingUser.Name = user.Name;
			existingUser.Email = user.Email;
			if (!string.IsNullOrEmpty(user.Password))

			{
				existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
			}


			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				return NotFound();
			}

			return NoContent();
		}
	}
}