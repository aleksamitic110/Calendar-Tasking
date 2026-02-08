using System.ComponentModel.DataAnnotations;

namespace CalendarTasking.Api.Contracts;

public sealed record EventResponse(
    int EventId,
    int CalendarId,
    int CreatedByUserId,
    string Title,
    string? Description,
    string? Location,
    DateTime StartUtc,
    DateTime EndUtc,
    bool IsAllDay,
    string RepeatType,
    int? ReminderMinutesBefore,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateEventRequest(
    int CalendarId,
    int CreatedByUserId,
    [param: Required, MaxLength(200)] string Title,
    [param: MaxLength(1000)] string? Description,
    [param: MaxLength(200)] string? Location,
    DateTime StartUtc,
    DateTime EndUtc,
    bool IsAllDay,
    [param: Required, MaxLength(20)] string RepeatType,
    int? ReminderMinutesBefore,
    [param: Required, MaxLength(20)] string Status);

public sealed record UpdateEventRequest(
    int CalendarId,
    int CreatedByUserId,
    [param: Required, MaxLength(200)] string Title,
    [param: MaxLength(1000)] string? Description,
    [param: MaxLength(200)] string? Location,
    DateTime StartUtc,
    DateTime EndUtc,
    bool IsAllDay,
    [param: Required, MaxLength(20)] string RepeatType,
    int? ReminderMinutesBefore,
    [param: Required, MaxLength(20)] string Status);

