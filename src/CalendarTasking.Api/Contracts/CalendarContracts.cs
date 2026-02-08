using System.ComponentModel.DataAnnotations;

namespace CalendarTasking.Api.Contracts;

public sealed record CalendarResponse(
    int CalendarId,
    int OwnerUserId,
    string Name,
    string? Description,
    string ColorHex,
    bool IsDefault,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateCalendarRequest(
    int OwnerUserId,
    [param: Required, MaxLength(120)] string Name,
    [param: MaxLength(500)] string? Description,
    [param: Required, RegularExpression("^#[0-9a-fA-F]{6}$")] string ColorHex,
    bool IsDefault);

public sealed record UpdateCalendarRequest(
    int OwnerUserId,
    [param: Required, MaxLength(120)] string Name,
    [param: MaxLength(500)] string? Description,
    [param: Required, RegularExpression("^#[0-9a-fA-F]{6}$")] string ColorHex,
    bool IsDefault);

