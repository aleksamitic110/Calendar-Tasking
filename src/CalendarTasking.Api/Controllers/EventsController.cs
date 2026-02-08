using CalendarTasking.Api.Contracts;
using CalendarTasking.Api.Data;
using CalendarTasking.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalendarTasking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(CalendarTaskingDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetEvents(
        [FromQuery] int? calendarId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc)
    {
        var query = dbContext.Events.AsNoTracking().AsQueryable();

        if (calendarId.HasValue)
        {
            query = query.Where(x => x.CalendarId == calendarId.Value);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.EndUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.StartUtc <= toUtc.Value);
        }

        var events = await query.OrderBy(x => x.StartUtc).ToListAsync();
        return Ok(events.Select(x => x.ToResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventResponse>> GetEvent(int id)
    {
        var calendarEvent = await dbContext.Events.AsNoTracking().FirstOrDefaultAsync(x => x.EventId == id);
        if (calendarEvent is null)
        {
            return NotFound();
        }

        return Ok(calendarEvent.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<EventResponse>> CreateEvent([FromBody] CreateEventRequest request)
    {
        var validationError = await ValidateEventRequest(request);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        AllowedValues.TryNormalizeEventRepeatType(request.RepeatType, out var repeatType);
        AllowedValues.TryNormalizeEventStatus(request.Status, out var status);

        var calendarEvent = new Event
        {
            CalendarId = request.CalendarId,
            CreatedByUserId = request.CreatedByUserId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            StartUtc = request.StartUtc,
            EndUtc = request.EndUtc,
            IsAllDay = request.IsAllDay,
            RepeatType = repeatType,
            ReminderMinutesBefore = request.ReminderMinutesBefore,
            Status = status,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Events.Add(calendarEvent);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEvent), new { id = calendarEvent.EventId }, calendarEvent.ToResponse());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EventResponse>> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        var calendarEvent = await dbContext.Events.FirstOrDefaultAsync(x => x.EventId == id);
        if (calendarEvent is null)
        {
            return NotFound();
        }

        var validationError = await ValidateEventRequest(request);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        AllowedValues.TryNormalizeEventRepeatType(request.RepeatType, out var repeatType);
        AllowedValues.TryNormalizeEventStatus(request.Status, out var status);

        calendarEvent.CalendarId = request.CalendarId;
        calendarEvent.CreatedByUserId = request.CreatedByUserId;
        calendarEvent.Title = request.Title.Trim();
        calendarEvent.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        calendarEvent.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
        calendarEvent.StartUtc = request.StartUtc;
        calendarEvent.EndUtc = request.EndUtc;
        calendarEvent.IsAllDay = request.IsAllDay;
        calendarEvent.RepeatType = repeatType;
        calendarEvent.ReminderMinutesBefore = request.ReminderMinutesBefore;
        calendarEvent.Status = status;
        calendarEvent.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok(calendarEvent.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var calendarEvent = await dbContext.Events.FirstOrDefaultAsync(x => x.EventId == id);
        if (calendarEvent is null)
        {
            return NotFound();
        }

        dbContext.Events.Remove(calendarEvent);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string?> ValidateEventRequest(CreateEventRequest request)
    {
        if (request.EndUtc <= request.StartUtc)
        {
            return "EndUtc must be greater than StartUtc.";
        }

        if (request.ReminderMinutesBefore is < 0)
        {
            return "ReminderMinutesBefore must be null or >= 0.";
        }

        if (!AllowedValues.TryNormalizeEventRepeatType(request.RepeatType, out _))
        {
            return "RepeatType must be one of: None, Daily, Weekly, Monthly.";
        }

        if (!AllowedValues.TryNormalizeEventStatus(request.Status, out _))
        {
            return "Status must be one of: Planned, Cancelled.";
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

    private Task<string?> ValidateEventRequest(UpdateEventRequest request)
    {
        var createEquivalent = new CreateEventRequest(
            request.CalendarId,
            request.CreatedByUserId,
            request.Title,
            request.Description,
            request.Location,
            request.StartUtc,
            request.EndUtc,
            request.IsAllDay,
            request.RepeatType,
            request.ReminderMinutesBefore,
            request.Status);

        return ValidateEventRequest(createEquivalent);
    }
}
