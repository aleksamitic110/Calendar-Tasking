using CalendarTasking.Api.Models;

namespace CalendarTasking.Api.Contracts;

public static class MappingExtensions
{
    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
    private static DateTime? AsUtc(DateTime? value) => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.TimeZoneId,
            user.IsActive,
            AsUtc(user.CreatedAtUtc),
            AsUtc(user.UpdatedAtUtc));
    }

    public static CalendarResponse ToResponse(this Calendar calendar)
    {
        return new CalendarResponse(
            calendar.CalendarId,
            calendar.OwnerUserId,
            calendar.Name,
            calendar.Description,
            calendar.ColorHex,
            calendar.IsDefault,
            AsUtc(calendar.CreatedAtUtc),
            AsUtc(calendar.UpdatedAtUtc));
    }

    public static EventResponse ToResponse(this Event calendarEvent)
    {
        return new EventResponse(
            calendarEvent.EventId,
            calendarEvent.CalendarId,
            calendarEvent.CreatedByUserId,
            calendarEvent.Title,
            calendarEvent.Description,
            calendarEvent.Location,
            AsUtc(calendarEvent.StartUtc),
            AsUtc(calendarEvent.EndUtc),
            calendarEvent.IsAllDay,
            calendarEvent.RepeatType,
            calendarEvent.ReminderMinutesBefore,
            calendarEvent.Status,
            AsUtc(calendarEvent.CreatedAtUtc),
            AsUtc(calendarEvent.UpdatedAtUtc));
    }

    public static TaskResponse ToResponse(this TaskItem task)
    {
        return new TaskResponse(
            task.TaskItemId,
            task.CalendarId,
            task.CreatedByUserId,
            task.Title,
            task.Description,
            AsUtc(task.DueUtc),
            task.Priority,
            task.Status,
            AsUtc(task.CompletedAtUtc),
            task.ReminderMinutesBefore,
            AsUtc(task.CreatedAtUtc),
            AsUtc(task.UpdatedAtUtc));
    }

    public static PrivateClassSessionResponse ToResponse(this PrivateClassSession session)
    {
        return new PrivateClassSessionResponse(
            session.PrivateClassSessionId,
            session.CalendarId,
            session.CreatedByUserId,
            session.StudentName,
            session.StudentContact,
            AsUtc(session.SessionStartUtc),
            AsUtc(session.SessionEndUtc),
            session.TopicPlanned,
            session.TopicDone,
            session.HomeworkAssigned,
            session.PriceAmount,
            session.CurrencyCode,
            session.IsPaid,
            AsUtc(session.PaidAtUtc),
            session.PaymentMethod,
            session.PaymentNote,
            session.Status,
            AsUtc(session.CreatedAtUtc),
            AsUtc(session.UpdatedAtUtc));
    }
}
