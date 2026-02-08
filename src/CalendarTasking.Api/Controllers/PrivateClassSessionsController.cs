using CalendarTasking.Api.Contracts;
using CalendarTasking.Api.Data;
using CalendarTasking.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalendarTasking.Api.Controllers;

[ApiController]
[Route("api/private-class-sessions")]
public class PrivateClassSessionsController(CalendarTaskingDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PrivateClassSessionResponse>>> GetSessions(
        [FromQuery] int? calendarId,
        [FromQuery] bool? isPaid,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc)
    {
        var query = dbContext.PrivateClassSessions.AsNoTracking().AsQueryable();

        if (calendarId.HasValue)
        {
            query = query.Where(x => x.CalendarId == calendarId.Value);
        }

        if (isPaid.HasValue)
        {
            query = query.Where(x => x.IsPaid == isPaid.Value);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.SessionEndUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.SessionStartUtc <= toUtc.Value);
        }

        var sessions = await query.OrderBy(x => x.SessionStartUtc).ToListAsync();
        return Ok(sessions.Select(x => x.ToResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PrivateClassSessionResponse>> GetSession(int id)
    {
        var session = await dbContext.PrivateClassSessions.AsNoTracking().FirstOrDefaultAsync(x => x.PrivateClassSessionId == id);
        if (session is null)
        {
            return NotFound();
        }

        return Ok(session.ToResponse());
    }

    [HttpGet("unpaid")]
    public async Task<ActionResult<IEnumerable<PrivateClassSessionResponse>>> GetUnpaidSessions([FromQuery] int? calendarId)
    {
        var query = dbContext.PrivateClassSessions
            .AsNoTracking()
            .Where(x => !x.IsPaid)
            .AsQueryable();

        if (calendarId.HasValue)
        {
            query = query.Where(x => x.CalendarId == calendarId.Value);
        }

        var sessions = await query.OrderBy(x => x.SessionStartUtc).ToListAsync();
        return Ok(sessions.Select(x => x.ToResponse()));
    }

    [HttpGet("monthly-summary")]
    public async Task<ActionResult<PrivateClassMonthlySummaryResponse>> GetMonthlySummary(
        [FromQuery] int calendarId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        if (year < 2000 || year > 2100)
        {
            return BadRequest("year must be between 2000 and 2100.");
        }

        if (month is < 1 or > 12)
        {
            return BadRequest("month must be between 1 and 12.");
        }

        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEndExclusive = monthStart.AddMonths(1);

        var monthQuery = dbContext.PrivateClassSessions
            .AsNoTracking()
            .Where(x => x.CalendarId == calendarId && x.SessionStartUtc >= monthStart && x.SessionStartUtc < monthEndExclusive);

        var totalSessions = await monthQuery.CountAsync();
        var paidSessions = await monthQuery.CountAsync(x => x.IsPaid);
        var unpaidSessions = totalSessions - paidSessions;

        var totalPaidAmount = await monthQuery
            .Where(x => x.IsPaid)
            .SumAsync(x => (decimal?)x.PriceAmount) ?? 0m;

        var totalScheduledAmount = await monthQuery.SumAsync(x => (decimal?)x.PriceAmount) ?? 0m;

        return Ok(new PrivateClassMonthlySummaryResponse(
            calendarId,
            year,
            month,
            totalPaidAmount,
            totalScheduledAmount,
            totalSessions,
            paidSessions,
            unpaidSessions));
    }

    [HttpPost]
    public async Task<ActionResult<PrivateClassSessionResponse>> CreateSession([FromBody] CreatePrivateClassSessionRequest request)
    {
        var validationError = await ValidateSessionRequest(request);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        AllowedValues.TryNormalizeSessionStatus(request.Status, out var status);
        AllowedValues.TryNormalizeSessionPaymentMethod(request.PaymentMethod, out var paymentMethod);

        var isPaid = request.IsPaid;
        DateTime? paidAtUtc = isPaid ? request.PaidAtUtc ?? DateTime.UtcNow : null;

        var session = new PrivateClassSession
        {
            CalendarId = request.CalendarId,
            CreatedByUserId = request.CreatedByUserId,
            StudentName = request.StudentName.Trim(),
            StudentContact = string.IsNullOrWhiteSpace(request.StudentContact) ? null : request.StudentContact.Trim(),
            SessionStartUtc = request.SessionStartUtc,
            SessionEndUtc = request.SessionEndUtc,
            TopicPlanned = string.IsNullOrWhiteSpace(request.TopicPlanned) ? null : request.TopicPlanned.Trim(),
            TopicDone = string.IsNullOrWhiteSpace(request.TopicDone) ? null : request.TopicDone.Trim(),
            HomeworkAssigned = string.IsNullOrWhiteSpace(request.HomeworkAssigned) ? null : request.HomeworkAssigned.Trim(),
            PriceAmount = request.PriceAmount,
            CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
            IsPaid = isPaid,
            PaidAtUtc = paidAtUtc,
            PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? null : paymentMethod,
            PaymentNote = string.IsNullOrWhiteSpace(request.PaymentNote) ? null : request.PaymentNote.Trim(),
            Status = status,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.PrivateClassSessions.Add(session);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSession), new { id = session.PrivateClassSessionId }, session.ToResponse());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PrivateClassSessionResponse>> UpdateSession(int id, [FromBody] UpdatePrivateClassSessionRequest request)
    {
        var session = await dbContext.PrivateClassSessions.FirstOrDefaultAsync(x => x.PrivateClassSessionId == id);
        if (session is null)
        {
            return NotFound();
        }

        var validationError = await ValidateSessionRequest(request);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        AllowedValues.TryNormalizeSessionStatus(request.Status, out var status);
        AllowedValues.TryNormalizeSessionPaymentMethod(request.PaymentMethod, out var paymentMethod);

        var isPaid = request.IsPaid;
        DateTime? paidAtUtc = isPaid ? request.PaidAtUtc ?? DateTime.UtcNow : null;

        session.CalendarId = request.CalendarId;
        session.CreatedByUserId = request.CreatedByUserId;
        session.StudentName = request.StudentName.Trim();
        session.StudentContact = string.IsNullOrWhiteSpace(request.StudentContact) ? null : request.StudentContact.Trim();
        session.SessionStartUtc = request.SessionStartUtc;
        session.SessionEndUtc = request.SessionEndUtc;
        session.TopicPlanned = string.IsNullOrWhiteSpace(request.TopicPlanned) ? null : request.TopicPlanned.Trim();
        session.TopicDone = string.IsNullOrWhiteSpace(request.TopicDone) ? null : request.TopicDone.Trim();
        session.HomeworkAssigned = string.IsNullOrWhiteSpace(request.HomeworkAssigned) ? null : request.HomeworkAssigned.Trim();
        session.PriceAmount = request.PriceAmount;
        session.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
        session.IsPaid = isPaid;
        session.PaidAtUtc = paidAtUtc;
        session.PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? null : paymentMethod;
        session.PaymentNote = string.IsNullOrWhiteSpace(request.PaymentNote) ? null : request.PaymentNote.Trim();
        session.Status = status;
        session.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok(session.ToResponse());
    }

    [HttpPut("{id:int}/mark-paid")]
    public async Task<ActionResult<PrivateClassSessionResponse>> MarkPaid(int id, [FromBody] MarkSessionPaidRequest request)
    {
        var session = await dbContext.PrivateClassSessions.FirstOrDefaultAsync(x => x.PrivateClassSessionId == id);
        if (session is null)
        {
            return NotFound();
        }

        if (!AllowedValues.TryNormalizeSessionPaymentMethod(request.PaymentMethod, out var normalizedMethod))
        {
            return BadRequest("PaymentMethod must be one of: Cash, Card, Transfer, or null.");
        }

        session.IsPaid = true;
        session.PaidAtUtc = request.PaidAtUtc ?? DateTime.UtcNow;
        session.PaymentMethod = string.IsNullOrWhiteSpace(normalizedMethod) ? null : normalizedMethod;
        session.PaymentNote = string.IsNullOrWhiteSpace(request.PaymentNote) ? null : request.PaymentNote.Trim();
        session.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok(session.ToResponse());
    }

    [HttpPut("{id:int}/mark-unpaid")]
    public async Task<ActionResult<PrivateClassSessionResponse>> MarkUnpaid(int id)
    {
        var session = await dbContext.PrivateClassSessions.FirstOrDefaultAsync(x => x.PrivateClassSessionId == id);
        if (session is null)
        {
            return NotFound();
        }

        session.IsPaid = false;
        session.PaidAtUtc = null;
        session.PaymentMethod = null;
        session.PaymentNote = null;
        session.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok(session.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var session = await dbContext.PrivateClassSessions.FirstOrDefaultAsync(x => x.PrivateClassSessionId == id);
        if (session is null)
        {
            return NotFound();
        }

        dbContext.PrivateClassSessions.Remove(session);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string?> ValidateSessionRequest(CreatePrivateClassSessionRequest request)
    {
        if (request.SessionEndUtc <= request.SessionStartUtc)
        {
            return "SessionEndUtc must be greater than SessionStartUtc.";
        }

        if (request.PriceAmount < 0)
        {
            return "PriceAmount must be >= 0.";
        }

        if (!AllowedValues.TryNormalizeSessionStatus(request.Status, out _))
        {
            return "Status must be one of: Scheduled, Completed, Cancelled, NoShow.";
        }

        if (!AllowedValues.TryNormalizeSessionPaymentMethod(request.PaymentMethod, out _))
        {
            return "PaymentMethod must be one of: Cash, Card, Transfer, or null.";
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

    private Task<string?> ValidateSessionRequest(UpdatePrivateClassSessionRequest request)
    {
        var createEquivalent = new CreatePrivateClassSessionRequest(
            request.CalendarId,
            request.CreatedByUserId,
            request.StudentName,
            request.StudentContact,
            request.SessionStartUtc,
            request.SessionEndUtc,
            request.TopicPlanned,
            request.TopicDone,
            request.HomeworkAssigned,
            request.PriceAmount,
            request.CurrencyCode,
            request.IsPaid,
            request.PaidAtUtc,
            request.PaymentMethod,
            request.PaymentNote,
            request.Status);

        return ValidateSessionRequest(createEquivalent);
    }
}
