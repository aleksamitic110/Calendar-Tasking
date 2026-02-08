using CalendarTasking.Api.Contracts;
using CalendarTasking.Api.Data;
using CalendarTasking.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalendarTasking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalendarsController(CalendarTaskingDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CalendarResponse>>> GetCalendars([FromQuery] int? ownerUserId)
    {
        var query = dbContext.Calendars.AsNoTracking().AsQueryable();
        if (ownerUserId.HasValue)
        {
            query = query.Where(x => x.OwnerUserId == ownerUserId.Value);
        }

        var calendars = await query.OrderBy(x => x.CalendarId).ToListAsync();
        return Ok(calendars.Select(x => x.ToResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CalendarResponse>> GetCalendar(int id)
    {
        var calendar = await dbContext.Calendars.AsNoTracking().FirstOrDefaultAsync(x => x.CalendarId == id);
        if (calendar is null)
        {
            return NotFound();
        }

        return Ok(calendar.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<CalendarResponse>> CreateCalendar([FromBody] CreateCalendarRequest request)
    {
        if (!await dbContext.Users.AnyAsync(x => x.UserId == request.OwnerUserId))
        {
            return BadRequest("Owner user does not exist.");
        }

        var normalizedName = request.Name.Trim();
        if (await dbContext.Calendars.AnyAsync(x => x.OwnerUserId == request.OwnerUserId && x.Name == normalizedName))
        {
            return Conflict("Calendar name must be unique per owner.");
        }

        if (request.IsDefault)
        {
            await ResetDefaultCalendarsForOwner(request.OwnerUserId);
        }

        var calendar = new Calendar
        {
            OwnerUserId = request.OwnerUserId,
            Name = normalizedName,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            ColorHex = request.ColorHex.ToUpperInvariant(),
            IsDefault = request.IsDefault,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Calendars.Add(calendar);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCalendar), new { id = calendar.CalendarId }, calendar.ToResponse());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CalendarResponse>> UpdateCalendar(int id, [FromBody] UpdateCalendarRequest request)
    {
        var calendar = await dbContext.Calendars.FirstOrDefaultAsync(x => x.CalendarId == id);
        if (calendar is null)
        {
            return NotFound();
        }

        if (!await dbContext.Users.AnyAsync(x => x.UserId == request.OwnerUserId))
        {
            return BadRequest("Owner user does not exist.");
        }

        var normalizedName = request.Name.Trim();
        if (await dbContext.Calendars.AnyAsync(x => x.CalendarId != id && x.OwnerUserId == request.OwnerUserId && x.Name == normalizedName))
        {
            return Conflict("Calendar name must be unique per owner.");
        }

        if (request.IsDefault)
        {
            await ResetDefaultCalendarsForOwner(request.OwnerUserId, id);
        }

        calendar.OwnerUserId = request.OwnerUserId;
        calendar.Name = normalizedName;
        calendar.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        calendar.ColorHex = request.ColorHex.ToUpperInvariant();
        calendar.IsDefault = request.IsDefault;
        calendar.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok(calendar.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCalendar(int id)
    {
        var calendar = await dbContext.Calendars.FirstOrDefaultAsync(x => x.CalendarId == id);
        if (calendar is null)
        {
            return NotFound();
        }

        dbContext.Calendars.Remove(calendar);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task ResetDefaultCalendarsForOwner(int ownerUserId, int? exceptCalendarId = null)
    {
        var calendarsToUnset = await dbContext.Calendars
            .Where(x => x.OwnerUserId == ownerUserId && x.IsDefault && (!exceptCalendarId.HasValue || x.CalendarId != exceptCalendarId.Value))
            .ToListAsync();

        foreach (var calendar in calendarsToUnset)
        {
            calendar.IsDefault = false;
            calendar.UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
