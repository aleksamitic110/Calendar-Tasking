using System.Net;
using System.Net.Http.Json;

namespace CalendarTasking.ComponentTests.Infrastructure;

public static class ApiClientExtensions
{
    public static async Task<UserResponseDto> RegisterUserAsync(this HttpClient client, string? email = null)
    {
        var request = new RegisterUserRequestDto(
            email ?? $"user-{Guid.NewGuid():N}@example.com",
            "Pass123!",
            "Test",
            "User",
            "UTC");

        var response = await client.PostAsJsonAsync("/api/users/register", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await response.Content.ReadAsStringAsync());

        var created = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.That(created, Is.Not.Null);
        return created!;
    }

    public static async Task<CalendarResponseDto> CreateCalendarAsync(this HttpClient client, int ownerUserId, string? name = null)
    {
        var request = new CreateCalendarRequestDto(
            ownerUserId,
            name ?? $"Calendar-{Guid.NewGuid():N}",
            "Component test calendar",
            "#157A6E",
            false);

        var response = await client.PostAsJsonAsync("/api/calendars", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await response.Content.ReadAsStringAsync());

        var created = await response.Content.ReadFromJsonAsync<CalendarResponseDto>();
        Assert.That(created, Is.Not.Null);
        return created!;
    }

    public static async Task<EventResponseDto> CreateEventAsync(this HttpClient client, int calendarId, int createdByUserId, string? title = null)
    {
        var now = DateTime.UtcNow;
        var request = new CreateEventRequestDto(
            calendarId,
            createdByUserId,
            title ?? $"Event-{Guid.NewGuid():N}",
            "Component event",
            "Home",
            now.AddHours(1),
            now.AddHours(2),
            false,
            "None",
            10,
            "Planned");

        var response = await client.PostAsJsonAsync("/api/events", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await response.Content.ReadAsStringAsync());

        var created = await response.Content.ReadFromJsonAsync<EventResponseDto>();
        Assert.That(created, Is.Not.Null);
        return created!;
    }

    public static async Task<TaskResponseDto> CreateTaskAsync(this HttpClient client, int calendarId, int createdByUserId, string? title = null)
    {
        var request = new CreateTaskRequestDto(
            calendarId,
            createdByUserId,
            title ?? $"Task-{Guid.NewGuid():N}",
            "Component task",
            DateTime.UtcNow.AddDays(1),
            "Medium",
            "Todo",
            null,
            15);

        var response = await client.PostAsJsonAsync("/api/tasks", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await response.Content.ReadAsStringAsync());

        var created = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.That(created, Is.Not.Null);
        return created!;
    }

    public static async Task<PrivateClassSessionResponseDto> CreatePrivateSessionAsync(this HttpClient client, int calendarId, int createdByUserId, string? studentName = null)
    {
        var now = DateTime.UtcNow;
        var request = new CreatePrivateClassSessionRequestDto(
            calendarId,
            createdByUserId,
            studentName ?? $"Student-{Guid.NewGuid():N}",
            "student@example.com",
            now.AddHours(1),
            now.AddHours(2),
            "Planned topic",
            null,
            null,
            1200m,
            "RSD",
            false,
            null,
            null,
            null,
            "Scheduled");

        var response = await client.PostAsJsonAsync("/api/private-class-sessions", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await response.Content.ReadAsStringAsync());

        var created = await response.Content.ReadFromJsonAsync<PrivateClassSessionResponseDto>();
        Assert.That(created, Is.Not.Null);
        return created!;
    }
}

public sealed record RegisterUserRequestDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? TimeZoneId);

public sealed record UserResponseDto(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string TimeZoneId,
    bool IsActive);

public sealed record UpdateUserRequestDto(
    string Email,
    string FirstName,
    string LastName,
    string? TimeZoneId,
    bool IsActive);

public sealed record CreateCalendarRequestDto(
    int OwnerUserId,
    string Name,
    string? Description,
    string ColorHex,
    bool IsDefault);

public sealed record CalendarResponseDto(
    int CalendarId,
    int OwnerUserId,
    string Name,
    string? Description,
    string ColorHex,
    bool IsDefault);

public sealed record UpdateCalendarRequestDto(
    int OwnerUserId,
    string Name,
    string? Description,
    string ColorHex,
    bool IsDefault);

public sealed record CreateEventRequestDto(
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
    string Status);

public sealed record EventResponseDto(
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
    string Status);

public sealed record UpdateEventRequestDto(
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
    string Status);

public sealed record CreateTaskRequestDto(
    int CalendarId,
    int CreatedByUserId,
    string Title,
    string? Description,
    DateTime? DueUtc,
    string Priority,
    string Status,
    DateTime? CompletedAtUtc,
    int? ReminderMinutesBefore);

public sealed record TaskResponseDto(
    int TaskItemId,
    int CalendarId,
    int CreatedByUserId,
    string Title,
    string? Description,
    DateTime? DueUtc,
    string Priority,
    string Status,
    DateTime? CompletedAtUtc,
    int? ReminderMinutesBefore);

public sealed record UpdateTaskRequestDto(
    int CalendarId,
    int CreatedByUserId,
    string Title,
    string? Description,
    DateTime? DueUtc,
    string Priority,
    string Status,
    DateTime? CompletedAtUtc,
    int? ReminderMinutesBefore);

public sealed record CreatePrivateClassSessionRequestDto(
    int CalendarId,
    int CreatedByUserId,
    string StudentName,
    string? StudentContact,
    DateTime SessionStartUtc,
    DateTime SessionEndUtc,
    string? TopicPlanned,
    string? TopicDone,
    string? HomeworkAssigned,
    decimal PriceAmount,
    string CurrencyCode,
    bool IsPaid,
    DateTime? PaidAtUtc,
    string? PaymentMethod,
    string? PaymentNote,
    string Status);

public sealed record PrivateClassSessionResponseDto(
    int PrivateClassSessionId,
    int CalendarId,
    int CreatedByUserId,
    string StudentName,
    string? StudentContact,
    DateTime SessionStartUtc,
    DateTime SessionEndUtc,
    string? TopicPlanned,
    string? TopicDone,
    string? HomeworkAssigned,
    decimal PriceAmount,
    string CurrencyCode,
    bool IsPaid,
    DateTime? PaidAtUtc,
    string? PaymentMethod,
    string? PaymentNote,
    string Status);

public sealed record UpdatePrivateClassSessionRequestDto(
    int CalendarId,
    int CreatedByUserId,
    string StudentName,
    string? StudentContact,
    DateTime SessionStartUtc,
    DateTime SessionEndUtc,
    string? TopicPlanned,
    string? TopicDone,
    string? HomeworkAssigned,
    decimal PriceAmount,
    string CurrencyCode,
    bool IsPaid,
    DateTime? PaidAtUtc,
    string? PaymentMethod,
    string? PaymentNote,
    string Status);
