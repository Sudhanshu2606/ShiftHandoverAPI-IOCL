using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftHandoverAPI.Data;
using ShiftHandoverAPI.Models;
using System.Security.Claims;

namespace ShiftHandoverAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("handover/{handoverId}")]
        public async Task<IActionResult> GetByHandover(int handoverId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Handover)
                .Where(t => t.HandoverId == handoverId)
                .ToListAsync();
            return Ok(tasks);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _context.Tasks
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Handover)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return Ok(tasks);
        }

        [HttpGet("my-tasks")]
        public async Task<IActionResult> GetMyTasks()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userShift = User.FindFirst("ShiftAssigned")?.Value;

            IQueryable<TaskItem> query = _context.Tasks
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Handover)
                .Where(t => t.AssignedTo == userId || t.CreatedBy == userId);

            if (userRole == "Supervisor")
            {
                query = query.Union(
                    _context.Tasks
                        .Include(t => t.AssignedUser)
                        .Include(t => t.CreatedByUser)
                        .Include(t => t.Handover)
                        .Where(t => t.Handover.ShiftName == userShift)
                );
            }

            var tasks = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("assigned-to-me")]
        public async Task<IActionResult> GetTasksAssignedToMe()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var tasks = await _context.Tasks
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Handover)
                .Where(t => t.AssignedTo == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return Ok(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskItem task)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            task.CreatedBy = userId;
            task.CreatedAt = DateTime.Now;
            task.Status = task.Status ?? "Pending";
            task.Priority = task.Priority ?? "Medium";

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Create history entry
            var history = new TaskHistory
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                HandoverId = task.HandoverId,
                Action = "Created",
                Details = $"Task '{task.Title}' was created",
                ChangedBy = userId,
                ChangedByName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown",
                ChangedAt = DateTime.Now
            };
            _context.TaskHistories.Add(history);
            await _context.SaveChangesAsync();

            return Ok(task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskItem updatedTask)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var changes = new List<string>();

            if (task.Title != updatedTask.Title)
            {
                changes.Add($"Title changed from '{task.Title}' to '{updatedTask.Title}'");
                task.Title = updatedTask.Title;
            }
            if (task.Description != updatedTask.Description)
            {
                changes.Add($"Description updated");
                task.Description = updatedTask.Description;
            }
            if (task.Priority != updatedTask.Priority)
            {
                changes.Add($"Priority changed from '{task.Priority}' to '{updatedTask.Priority}'");
                task.Priority = updatedTask.Priority;
            }
            if (task.Status != updatedTask.Status)
            {
                changes.Add($"Status changed from '{task.Status}' to '{updatedTask.Status}'");
                task.Status = updatedTask.Status;
                if (updatedTask.Status == "Completed")
                    task.CompletedAt = DateTime.Now;
            }
            if (task.AssignedTo != updatedTask.AssignedTo)
            {
                var oldUser = await _context.Users.FindAsync(task.AssignedTo);
                var newUser = await _context.Users.FindAsync(updatedTask.AssignedTo);
                changes.Add($"Reassigned from '{oldUser?.Name ?? "Unassigned"}' to '{newUser?.Name ?? "Unassigned"}'");
                task.AssignedTo = updatedTask.AssignedTo;
            }
            if (task.DueDate != updatedTask.DueDate)
            {
                changes.Add($"Due date changed from '{task.DueDate:yyyy-MM-dd}' to '{updatedTask.DueDate:yyyy-MM-dd}'");
                task.DueDate = updatedTask.DueDate;
            }
            if (task.Remarks != updatedTask.Remarks)
            {
                changes.Add($"Remarks updated");
                task.Remarks = updatedTask.Remarks;
            }

            await _context.SaveChangesAsync();

            if (changes.Count > 0)
            {
                var history = new TaskHistory
                {
                    TaskId = task.Id,
                    TaskTitle = task.Title,
                    HandoverId = task.HandoverId,
                    Action = "Updated",
                    Details = string.Join("; ", changes),
                    ChangedBy = userId,
                    ChangedByName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown",
                    ChangedAt = DateTime.Now
                };
                _context.TaskHistories.Add(history);
                await _context.SaveChangesAsync();
            }

            return Ok(task);
        }

        // PUT: api/task/{id}/status - Updated with Remarks support
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (task == null) return NotFound();

            var oldStatus = task.Status;
            task.Status = dto.Status;
            
            // Update remarks if provided
            if (!string.IsNullOrEmpty(dto.Remarks))
            {
                task.Remarks = dto.Remarks;
            }

            if (dto.Status == "Completed")
                task.CompletedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var history = new TaskHistory
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                HandoverId = task.HandoverId,
                Action = "StatusChanged",
                Details = $"Status changed from '{oldStatus}' to '{dto.Status}'",
                OldValue = oldStatus,
                NewValue = dto.Status,
                ChangedBy = userId,
                ChangedByName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown",
                ChangedAt = DateTime.Now
            };

            if (!string.IsNullOrEmpty(dto.Remarks))
            {
                history.Details += $" | Remarks: {dto.Remarks}";
            }

            _context.TaskHistories.Add(history);
            await _context.SaveChangesAsync();

            // Return updated task with all details
            var updatedTask = await _context.Tasks
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Handover)
                .FirstOrDefaultAsync(t => t.Id == id);

            return Ok(updatedTask);
        }

        [HttpPut("{id}/reassign")]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> ReassignTask(int id, [FromBody] ReassignDto dto)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            var oldUser = task.AssignedUser?.Name ?? "Unassigned";
            var newUser = await _context.Users.FindAsync(dto.NewAssignedTo);
            if (newUser == null) return BadRequest("User not found");

            task.AssignedTo = dto.NewAssignedTo;
            await _context.SaveChangesAsync();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var history = new TaskHistory
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                HandoverId = task.HandoverId,
                Action = "Reassigned",
                Details = $"Reassigned from '{oldUser}' to '{newUser.Name}'",
                OldValue = oldUser,
                NewValue = newUser.Name,
                ChangedBy = userId,
                ChangedByName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown",
                ChangedAt = DateTime.Now
            };
            _context.TaskHistories.Add(history);
            await _context.SaveChangesAsync();

            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var history = new TaskHistory
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                HandoverId = task.HandoverId,
                Action = "Deleted",
                Details = $"Task '{task.Title}' was deleted",
                ChangedBy = userId,
                ChangedByName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown",
                ChangedAt = DateTime.Now
            };
            _context.TaskHistories.Add(history);

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
    }

    public class StatusUpdateDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class ReassignDto
    {
        public int NewAssignedTo { get; set; }
    }
}