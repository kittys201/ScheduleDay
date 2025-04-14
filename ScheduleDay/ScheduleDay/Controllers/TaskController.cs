using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ScheduleDay.Data;
using ScheduleDay.Models;
using ScheduleDay.Shared.Models;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ScheduleDay.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class TasksController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IMemoryCache _cache;

		public TasksController(AppDbContext context, IHttpClientFactory httpClientFactory, IMemoryCache cache)
		{
			_cache = cache;
			_context = context;
			_httpClientFactory = httpClientFactory;
		}

		private int GetUserId()
		{
			// Get authenticated user's id
			return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
		{
			var taskItems = new List<TaskItem>();
			var userId = GetUserId();

			//Get Google Events if Google is the auth provider
			var provider = User.FindFirst("auth_provider")?.Value;
			if (provider == "Google")
			{
				var email = User.FindFirst(ClaimTypes.Email)?.Value;
				_cache.TryGetValue($"google_token_{email}", out string accessToken);

				// Client
				var httpClient = _httpClientFactory.CreateClient();

				// Header
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

				// Request
				var response = await httpClient.GetAsync("https://www.googleapis.com/calendar/v3/calendars/primary/events");
				if (response.IsSuccessStatusCode)
				{
					var json = await response.Content.ReadAsStringAsync();
					var calendarResponse = JsonSerializer.Deserialize<GoogleCalendarResponse>(json, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					//Get only tasks from this month
					var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
					taskItems = calendarResponse.Items
					.Where(x => x.Start.EventDate > thisMonth)
					.Select(item => new TaskItem
					{
						Name = item.Summary ?? "Sin título",
						Description = item.Description,
						Date = item.Start?.DateTime ?? DateTime.UtcNow,
						Status = "Imported",
						UserID = userId,
						GoogleEvent = true,
					}).ToList();
				}
			}

			// Get authenticated user's tasks
			try
			{
				var appTasks = await _context.TaskItems
												.Where(t => t.UserID == userId)
												.OrderBy(t => t.Date)
												.ToListAsync();
				taskItems.AddRange(appTasks);
			}
			catch (Exception ex)
			{
			}

			return taskItems;
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<TaskItem>> GetTask(int id)
		{
			// Get task by id
			var userId = GetUserId();
			var task = await _context.TaskItems
								   .FirstOrDefaultAsync(t => t.ID == id && t.UserID == userId);

			if (task == null)
			{
				return NotFound();
			}

			return task;
		}

		[HttpPost]
		public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
		{
			// Create new task
			task.UserID = GetUserId();
			task.Status = task.Status;
			task.Date = DateTime.SpecifyKind(task.Date, DateTimeKind.Utc);

			_context.TaskItems.Add(task);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetTask), new { id = task.ID }, task);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateTask(int id, TaskItem task)
		{
			// Update task by id
			if (id != task.ID || task.GoogleEvent is true)
			{
				return BadRequest();
			}

			var userId = GetUserId();
			var existingTask = await _context.TaskItems
										   .FirstOrDefaultAsync(t => t.ID == id && t.UserID == userId);

			if (existingTask == null)
			{
				return NotFound();
			}

			existingTask.Name = task.Name;
			existingTask.Description = task.Description;
			existingTask.Date = DateTime.SpecifyKind(task.Date, DateTimeKind.Utc);
			existingTask.Status = task.Status;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!TaskExists(id))
				{
					return NotFound();
				}
				throw;
			}

			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteTask(int id)
		{
			// Delete task by id
			var userId = GetUserId();
			var task = await _context.TaskItems
								   .FirstOrDefaultAsync(t => t.ID == id && t.UserID == userId);

			if (task == null)
			{
				return NotFound();
			}

			_context.TaskItems.Remove(task);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool TaskExists(int id)
		{
			// Helper function to check if task with specific id exists
			return _context.TaskItems.Any(e => e.ID == id);
		}
	}
}