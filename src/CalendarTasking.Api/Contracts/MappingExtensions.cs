using CalendarTasking.Api.Models;

namespace CalendarTasking.Api.Contracts;

public static class MappingExtensions
{
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.TimeZoneId,
            user.IsActive,
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
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
            calendar.CreatedAtUtc,
            calendar.UpdatedAtUtc);
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
            calendarEvent.StartUtc,
            calendarEvent.EndUtc,
            calendarEvent.IsAllDay,
            calendarEvent.RepeatType,
            calendarEvent.ReminderMinutesBefore,
            calendarEvent.Status,
            calendarEvent.CreatedAtUtc,
            calendarEvent.UpdatedAtUtc);
    }

    public static TaskResponse ToResponse(this TaskItem task)
    {
        return new TaskResponse(
            task.TaskItemId,
            task.CalendarId,
            task.CreatedByUserId,
            task.Title,
            task.Description,
            task.DueUtc,
            task.Priority,
            task.Status,
            task.CompletedAtUtc,
            task.ReminderMinutesBefore,
            task.CreatedAtUtc,
            task.UpdatedAtUtc);
    }

    public static PrivateClassSessionResponse ToResponse(this PrivateClassSession session)
    {
        return new PrivateClassSessionResponse(
            session.PrivateClassSessionId,
            session.CalendarId,
            session.CreatedByUserId,
            session.StudentName,
            session.StudentContact,
            session.SessionStartUtc,
            session.SessionEndUtc,
            session.TopicPlanned,
            session.TopicDone,
            session.HomeworkAssigned,
            session.PriceAmount,
            session.CurrencyCode,
            session.IsPaid,
            session.PaidAtUtc,
            session.PaymentMethod,
            session.PaymentNote,
            session.Status,
            session.CreatedAtUtc,
            session.UpdatedAtUtc);
    }
}
