namespace CalendarTasking.Api.Models;

public class TaskItem
{
    public int TaskItemId { get; set; }
    public int CalendarId { get; set; }
    public int CreatedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueUtc { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Todo";
    public DateTime? CompletedAtUtc { get; set; }
    public int? ReminderMinutesBefore { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Calendar Calendar { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}
