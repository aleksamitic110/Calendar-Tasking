using System.ComponentModel.DataAnnotations;

namespace CalendarTasking.Api.Contracts;

public sealed record UserResponse(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string TimeZoneId,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record RegisterUserRequest(
    [param: Required, EmailAddress, MaxLength(255)] string Email,
    [param: Required, MinLength(6), MaxLength(100)] string Password,
    [param: Required, MaxLength(80)] string FirstName,
    [param: Required, MaxLength(80)] string LastName,
    [param: MaxLength(64)] string? TimeZoneId);

public sealed record UpdateUserRequest(
    [param: Required, EmailAddress, MaxLength(255)] string Email,
    [param: Required, MaxLength(80)] string FirstName,
    [param: Required, MaxLength(80)] string LastName,
    [param: MaxLength(64)] string? TimeZoneId,
    bool IsActive);

public sealed record LoginUserRequest(
    [param: Required, EmailAddress, MaxLength(255)] string Email,
    [param: Required, MinLength(6), MaxLength(100)] string Password);

public sealed record ChangePasswordRequest(
    [param: Required, MinLength(6), MaxLength(100)] string CurrentPassword,
    [param: Required, MinLength(6), MaxLength(100)] string NewPassword);

public sealed record LoginUserResponse(int UserId, string Email, string FullName);

