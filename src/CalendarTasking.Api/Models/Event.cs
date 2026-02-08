namespace CalendarTasking.Api.Models;

public class Event
{
    public int EventId { get; set; }
    public int CalendarId { get; set; }
    public int CreatedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public bool IsAllDay { get; set; }
    public string RepeatType { get; set; } = "None";
    public int? ReminderMinutesBefore { get; set; }
    public string Status { get; set; } = "Planned";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Calendar Calendar { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}
