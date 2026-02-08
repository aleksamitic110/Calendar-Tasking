namespace CalendarTasking.Api.Models;

public class Calendar
{
    public int CalendarId { get; set; }
    public int OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ColorHex { get; set; } = "#2563EB";
    public bool IsDefault { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public User OwnerUser { get; set; } = null!;
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<PrivateClassSession> PrivateClassSessions { get; set; } = new List<PrivateClassSession>();
}
