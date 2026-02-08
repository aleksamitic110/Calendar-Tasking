using System.ComponentModel.DataAnnotations;

namespace CalendarTasking.Api.Contracts;

public sealed record PrivateClassSessionResponse(
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
    string Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreatePrivateClassSessionRequest(
    int CalendarId,
    int CreatedByUserId,
    [param: Required, MaxLength(120)] string StudentName,
    [param: MaxLength(120)] string? StudentContact,
    DateTime SessionStartUtc,
    DateTime SessionEndUtc,
    [param: MaxLength(500)] string? TopicPlanned,
    [param: MaxLength(1500)] string? TopicDone,
    [param: MaxLength(1500)] string? HomeworkAssigned,
    decimal PriceAmount,
    [param: Required, StringLength(3, MinimumLength = 3)] string CurrencyCode,
    bool IsPaid,
    DateTime? PaidAtUtc,
    [param: MaxLength(20)] string? PaymentMethod,
    [param: MaxLength(500)] string? PaymentNote,
    [param: Required, MaxLength(20)] string Status);

public sealed record UpdatePrivateClassSessionRequest(
    int CalendarId,
    int CreatedByUserId,
    [param: Required, MaxLength(120)] string StudentName,
    [param: MaxLength(120)] string? StudentContact,
    DateTime SessionStartUtc,
    DateTime SessionEndUtc,
    [param: MaxLength(500)] string? TopicPlanned,
    [param: MaxLength(1500)] string? TopicDone,
    [param: MaxLength(1500)] string? HomeworkAssigned,
    decimal PriceAmount,
    [param: Required, StringLength(3, MinimumLength = 3)] string CurrencyCode,
    bool IsPaid,
    DateTime? PaidAtUtc,
    [param: MaxLength(20)] string? PaymentMethod,
    [param: MaxLength(500)] string? PaymentNote,
    [param: Required, MaxLength(20)] string Status);

public sealed record MarkSessionPaidRequest(
    [param: MaxLength(20)] string? PaymentMethod,
    [param: MaxLength(500)] string? PaymentNote,
    DateTime? PaidAtUtc);

public sealed record PrivateClassMonthlySummaryResponse(
    int CalendarId,
    int Year,
    int Month,
    decimal TotalPaidAmount,
    decimal TotalScheduledAmount,
    int TotalSessions,
    int PaidSessions,
    int UnpaidSessions);

