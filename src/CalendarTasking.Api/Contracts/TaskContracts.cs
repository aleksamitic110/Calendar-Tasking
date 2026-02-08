using System.ComponentModel.DataAnnotations;

namespace CalendarTasking.Api.Contracts;

public sealed record TaskResponse(
    int TaskItemId,
    int CalendarId,
    int CreatedByUserId,
    string Title,
    string? Description,
    DateTime? DueUtc,
    string Priority,
    string Status,
    DateTime? CompletedAtUtc,
    int? ReminderMinutesBefore,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateTaskRequest(
    int CalendarId,
    int CreatedByUserId,
    [param: Required, MaxLength(200)] string Title,
    [param: MaxLength(1000)] string? Description,
    DateTime? DueUtc,
    [param: Required, MaxLength(20)] string Priority,
    [param: Required, MaxLength(20)] string Status,
    DateTime? CompletedAtUtc,
    int? ReminderMinutesBefore);

public sealed record UpdateTaskRequest(
    int CalendarId,
    int CreatedByUserId,
    [param: Required, MaxLength(200)] string Title,
    [param: MaxLength(1000)] string? Description,
    DateTime? DueUtc,
    [param: Required, MaxLength(20)] string Priority,
    [param: Required, MaxLength(20)] string Status,
    DateTime? CompletedAtUtc,
    int? ReminderMinutesBefore);

public sealed record UpdateTaskStatusRequest(
    [param: Required, MaxLength(20)] string Status,
    DateTime? CompletedAtUtc);

