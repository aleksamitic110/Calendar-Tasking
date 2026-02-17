using System.Text.Json.Nodes;
using Microsoft.Playwright;

namespace CalendarTasking.PlaywrightTests.Infrastructure;

public abstract class PlaywrightApiTestBase
{
    private IPlaywright _playwright = null!;

    protected IAPIRequestContext Api = null!;

    [SetUp]
    public async Task SetUpApiContext()
    {
        _playwright = await Playwright.CreateAsync();
        Api = await _playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = TestConfig.BaseUrl,
            IgnoreHTTPSErrors = true
        });
    }

    [TearDown]
    public async Task TearDownApiContext()
    {
        await Api.DisposeAsync();
        _playwright.Dispose();
    }

    protected static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    protected static async Task<JsonNode?> ReadJsonAsync(IAPIResponse response)
    {
        var body = await response.TextAsync();
        return string.IsNullOrWhiteSpace(body) ? null : JsonNode.Parse(body);
    }

    protected static int JsonInt(JsonNode? node, string key) => node![key]!.GetValue<int>();

    protected static string JsonString(JsonNode? node, string key) => node![key]!.GetValue<string>();

    protected static DateTime FutureUtc(int hoursFromNow) => DateTime.UtcNow.AddHours(hoursFromNow);

    protected async Task<(int UserId, string Email)> RegisterUserAsync(string? email = null, string? password = null)
    {
        var userEmail = email ?? $"{Unique("pw-user")}@example.com";
        var pwd = password ?? "Pass123!";

        var response = await Api.PostAsync("/api/users/register", new APIRequestContextOptions
        {
            DataObject = new
            {
                email = userEmail,
                password = pwd,
                firstName = "Playwright",
                lastName = "User",
                timeZoneId = "UTC"
            }
        });

        Assert.That(response.Status, Is.EqualTo(201), await response.TextAsync());
        var json = await ReadJsonAsync(response);
        return (JsonInt(json, "userId"), JsonString(json, "email"));
    }

    protected async Task<int> CreateCalendarAsync(int ownerUserId, string? name = null, bool isDefault = false)
    {
        var response = await Api.PostAsync("/api/calendars", new APIRequestContextOptions
        {
            DataObject = new
            {
                ownerUserId,
                name = name ?? Unique("pw-calendar"),
                description = "Playwright calendar",
                colorHex = "#157A6E",
                isDefault
            }
        });

        Assert.That(response.Status, Is.EqualTo(201), await response.TextAsync());
        var json = await ReadJsonAsync(response);
        return JsonInt(json, "calendarId");
    }

    protected async Task<int> CreateTaskAsync(int calendarId, int createdByUserId, string? title = null)
    {
        var response = await Api.PostAsync("/api/tasks", new APIRequestContextOptions
        {
            DataObject = new
            {
                calendarId,
                createdByUserId,
                title = title ?? Unique("pw-task"),
                description = "Playwright task",
                dueUtc = FutureUtc(24).ToString("O"),
                priority = "Medium",
                status = "Todo",
                completedAtUtc = (string?)null,
                reminderMinutesBefore = 15
            }
        });

        Assert.That(response.Status, Is.EqualTo(201), await response.TextAsync());
        var json = await ReadJsonAsync(response);
        return JsonInt(json, "taskItemId");
    }

    protected async Task<int> CreateEventAsync(int calendarId, int createdByUserId, string? title = null)
    {
        var start = FutureUtc(2);
        var end = FutureUtc(3);

        var response = await Api.PostAsync("/api/events", new APIRequestContextOptions
        {
            DataObject = new
            {
                calendarId,
                createdByUserId,
                title = title ?? Unique("pw-event"),
                description = "Playwright event",
                location = "Lab",
                startUtc = start.ToString("O"),
                endUtc = end.ToString("O"),
                isAllDay = false,
                repeatType = "None",
                reminderMinutesBefore = 20,
                status = "Planned"
            }
        });

        Assert.That(response.Status, Is.EqualTo(201), await response.TextAsync());
        var json = await ReadJsonAsync(response);
        return JsonInt(json, "eventId");
    }

    protected async Task<int> CreateSessionAsync(int calendarId, int createdByUserId, bool isPaid = false, string? student = null)
    {
        var start = FutureUtc(4);
        var end = FutureUtc(5);

        var response = await Api.PostAsync("/api/private-class-sessions", new APIRequestContextOptions
        {
            DataObject = new
            {
                calendarId,
                createdByUserId,
                studentName = student ?? Unique("pw-student"),
                studentContact = "student@example.com",
                sessionStartUtc = start.ToString("O"),
                sessionEndUtc = end.ToString("O"),
                topicPlanned = "Topic",
                topicDone = (string?)null,
                homeworkAssigned = "Homework",
                priceAmount = 2100.00m,
                currencyCode = "RSD",
                isPaid,
                paidAtUtc = isPaid ? FutureUtc(-1).ToString("O") : null,
                paymentMethod = isPaid ? "Cash" : null,
                paymentNote = isPaid ? "Paid." : null,
                status = "Scheduled"
            }
        });

        Assert.That(response.Status, Is.EqualTo(201), await response.TextAsync());
        var json = await ReadJsonAsync(response);
        return JsonInt(json, "privateClassSessionId");
    }
}
