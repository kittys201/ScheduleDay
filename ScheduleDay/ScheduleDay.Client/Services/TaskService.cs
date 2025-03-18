using System.Net.Http.Json;
using ScheduleDay.Shared.Models;
using Microsoft.Extensions.Logging;

namespace ScheduleDay.Client.Services
{
    public class TaskService
    {
        private readonly HttpClient _http;
        private readonly ILogger<TaskService> _logger;

        public TaskService(HttpClient http, ILogger<TaskService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<TaskItem>> GetTasks()
        {
            try 
            {
                return await _http.GetFromJsonAsync<List<TaskItem>>("api/tasks") ?? new List<TaskItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks");
                throw;
            }
        }

        public async Task<TaskItem?> GetTaskById(int id)
        {
            try 
            {
                return await _http.GetFromJsonAsync<TaskItem>($"api/tasks/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting task {id}");
                throw;
            }
        }

        public async Task<bool> CreateTask(TaskItem task)
        {
            try 
            {
                var response = await _http.PostAsJsonAsync("api/tasks", task);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error creating task: {error}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                throw;
            }
        }

        public async Task<bool> UpdateTask(TaskItem task)
        {
            try 
            {
                var response = await _http.PutAsJsonAsync($"api/tasks/{task.ID}", task);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error updating task: {error}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task {task.ID}");
                throw;
            }
        }

        public async Task<bool> DeleteTask(int id)
        {
            try 
            {
                var response = await _http.DeleteAsync($"api/tasks/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error deleting task: {error}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting task {id}");
                throw;
            }
        }
    }
}