using CalendarTasking.Api.Contracts;
using CalendarTasking.Api.Data;
using CalendarTasking.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalendarTasking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(CalendarTaskingDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> GetTasks(
        [FromQuery] int? calendarId,
        [FromQuery] string? status,
        [FromQuery] DateTime? dueBeforeUtc)
    {
        var query = dbContext.Tasks.AsNoTracking().AsQueryable();

        if (calendarId.HasValue)
        {
            query = query.Where(x => x.CalendarId == calendarId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!AllowedValues.TryNormalizeTaskStatus(status, out var normalizedStatus))
            {
                return BadRequest("Status must be one of: Todo, InProgress, Done.");
            }

            query = query.Where(x => x.Status == normalizedStatus);
        }

        if (dueBeforeUtc.HasValue)
        {
            query = query.Where(x => x.DueUtc.HasValue && x.DueUtc <= dueBeforeUtc.Value);
        }

        var tasks = await query.OrderBy(x => x.DueUtc).ThenBy(x => x.TaskItemId).ToListAsync();
        return Ok(tasks.Select(x => x.ToResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> GetTask(int id)
    {
        var task = await dbContext.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.TaskItemId == id);
        if (task is null)
        {
            return NotFound();
        }

        return Ok(task.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> CreateTask([FromBody] CreateTaskRequest request)
    {
        var validationError = await ValidateTaskRequest(request);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        AllowedValues.TryNormalizeTaskPriority(request.Priority, out var priority);
        AllowedValues.TryNormalizeTaskStatus(request.Status, out var status);
        var completedAtUtc = ResolveCompletedAtUtc(status, request.CompletedAtUtc);

        var task = new TaskItem
        {
            CalendarId = request.CalendarId,
            CreatedByUserId = request.CreatedByUserId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DueUtc = request.DueUtc,
            Priority = priority,
            Status = status,
            CompletedAtUtc = completedAtUtc,
            ReminderMinutesBefore = request.ReminderMinutesBefore,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.TaskItemId }, task.ToResponse());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponse>> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
    {
        var task = await dbContext.Tasks.FirstOrDefaultAsync(x => x.TaskItemId == id);
        if (task is null)
        {
            return NotFound();
        }

        var validationError = await ValidateTaskRequest(request);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        AllowedValues.TryNormalizeTaskPriority(request.Priority, out var priority);
        AllowedValues.TryNormalizeTaskStatus(request.Status, out var status);
        var completedAtUtc = ResolveCompletedAtUtc(status, request.CompletedAtUtc);

        task.CalendarId = request.CalendarId;
        task.CreatedByUserId = request.CreatedByUserId;
        task.Title = request.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        task.DueUtc = request.DueUtc;
        task.Priority = priority;
        task.Status = status;
        task.CompletedAtUtc = completedAtUtc;
        task.ReminderMinutesBefore = request.ReminderMinutesBefore;
        task.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok(task.ToResponse());
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<TaskResponse>> UpdateTaskStatus(int id, [FromBody] UpdateTaskStatusRequest request)
    {
        var task = await dbContext.Tasks.FirstOrDefaultAsync(x => x.TaskItemId == id);
        if (task is null)
        {
            return NotFound();
        }

        if (!AllowedValues.TryNormalizeTaskStatus(request.Status, out var normalizedStatus))
        {
            return BadRequest("Status must be one of: Todo, InProgress, Done.");
        }

        task.Status = normalizedStatus;
        task.CompletedAtUtc = ResolveCompletedAtUtc(normalizedStatus, request.CompletedAtUtc);
        task.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok(task.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await dbContext.Tasks.FirstOrDefaultAsync(x => x.TaskItemId == id);
        if (task is null)
        {
            return NotFound();
        }

        dbContext.Tasks.Remove(task);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string?> ValidateTaskRequest(CreateTaskRequest request)
    {
        if (request.ReminderMinutesBefore is < 0)
        {
            return "ReminderMinutesBefore must be null or >= 0.";
        }

        if (!AllowedValues.TryNormalizeTaskPriority(request.Priority, out _))
        {
            return "Priority must be one of: Low, Medium, High.";
        }

        if (!AllowedValues.TryNormalizeTaskStatus(request.Status, out _))
        {
            return "Status must be one of: Todo, InProgress, Done.";
        }

        if (!await dbContext.Calendars.AnyAsync(x => x.CalendarId == request.CalendarId))
        {
            return "Calendar does not exist.";
        }

        if (!await dbContext.Users.AnyAsync(x => x.UserId == request.CreatedByUserId))
        {
            return "CreatedByUser does not exist.";
        }

        return null;
    }

    private Task<string?> ValidateTaskRequest(UpdateTaskRequest request)
    {
        var createEquivalent = new CreateTaskRequest(
            request.CalendarId,
            request.CreatedByUserId,
            request.Title,
            request.Description,
            request.DueUtc,
            request.Priority,
            request.Status,
            request.CompletedAtUtc,
            request.ReminderMinutesBefore);

        return ValidateTaskRequest(createEquivalent);
    }

    private static DateTime? ResolveCompletedAtUtc(string normalizedStatus, DateTime? requestedCompletedAtUtc)
    {
        if (!string.Equals(normalizedStatus, "Done", StringComparison.Ordinal))
        {
            return null;
        }

        return requestedCompletedAtUtc ?? DateTime.UtcNow;
    }
}
