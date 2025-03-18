using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ScheduleDay.Data;
using ScheduleDay.Shared.Models; 

namespace ScheduleDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            // Get authenticated user's id
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            // Get authenticated user's tasks
            var userId = GetUserId();
            return await _context.TaskItems
                                .Where(t => t.UserID == userId)
                                .OrderBy(t => t.Date)
                                .ToListAsync();
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
            task.Status = "Pending";
            task.Date = DateTime.SpecifyKind(task.Date, DateTimeKind.Utc);

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.ID }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem task)
        {
            // Update task by id
            if (id != task.ID)
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