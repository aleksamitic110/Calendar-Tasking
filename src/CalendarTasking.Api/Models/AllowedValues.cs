namespace CalendarTasking.Api.Models;

public static class AllowedValues
{
    private static readonly string[] EventRepeatTypes = ["None", "Daily", "Weekly", "Monthly"];
    private static readonly string[] EventStatuses = ["Planned", "Cancelled"];
    private static readonly string[] TaskPriorities = ["Low", "Medium", "High"];
    private static readonly string[] TaskStatuses = ["Todo", "InProgress", "Done"];
    private static readonly string[] SessionPaymentMethods = ["Cash", "Card", "Transfer"];
    private static readonly string[] SessionStatuses = ["Scheduled", "Completed", "Cancelled", "NoShow"];

    public static bool TryNormalizeEventRepeatType(string? value, out string normalized)
    {
        return TryNormalize(value, EventRepeatTypes, out normalized);
    }

    public static bool TryNormalizeEventStatus(string? value, out string normalized)
    {
        return TryNormalize(value, EventStatuses, out normalized);
    }

    public static bool TryNormalizeTaskPriority(string? value, out string normalized)
    {
        return TryNormalize(value, TaskPriorities, out normalized);
    }

    public static bool TryNormalizeTaskStatus(string? value, out string normalized)
    {
        return TryNormalize(value, TaskStatuses, out normalized);
    }

    public static bool TryNormalizeSessionPaymentMethod(string? value, out string normalized)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            normalized = string.Empty;
            return true;
        }

        return TryNormalize(value, SessionPaymentMethods, out normalized);
    }

    public static bool TryNormalizeSessionStatus(string? value, out string normalized)
    {
        return TryNormalize(value, SessionStatuses, out normalized);
    }

    private static bool TryNormalize(string? value, IReadOnlyCollection<string> allowedValues, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (var allowedValue in allowedValues)
        {
            if (string.Equals(allowedValue, value.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                normalized = allowedValue;
                return true;
            }
        }

        return false;
    }
}
