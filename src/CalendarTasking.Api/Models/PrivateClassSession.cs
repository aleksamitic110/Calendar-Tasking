namespace CalendarTasking.Api.Models;

public class PrivateClassSession
{
    public int PrivateClassSessionId { get; set; }
    public int CalendarId { get; set; }
    public int CreatedByUserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentContact { get; set; }
    public DateTime SessionStartUtc { get; set; }
    public DateTime SessionEndUtc { get; set; }
    public string? TopicPlanned { get; set; }
    public string? TopicDone { get; set; }
    public string? HomeworkAssigned { get; set; }
    public decimal PriceAmount { get; set; }
    public string CurrencyCode { get; set; } = "RSD";
    public bool IsPaid { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentNote { get; set; }
    public string Status { get; set; } = "Scheduled";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Calendar Calendar { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}
