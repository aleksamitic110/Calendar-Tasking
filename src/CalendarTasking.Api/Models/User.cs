namespace CalendarTasking.Api.Models;

public class User
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = "UTC";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<Calendar> OwnedCalendars { get; set; } = new List<Calendar>();
    public ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
    public ICollection<PrivateClassSession> CreatedPrivateClassSessions { get; set; } = new List<PrivateClassSession>();
}
