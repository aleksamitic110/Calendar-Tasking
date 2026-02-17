using CalendarTasking.PlaywrightTests.Infrastructure;
using Microsoft.Playwright;

namespace CalendarTasking.PlaywrightTests.Api;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class PrivateClassSessionsApiTests : PlaywrightApiTestBase
{
    [Test]
    public async Task GetSessions_ShouldReturnCalendarSessions()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var sessionId = await CreateSessionAsync(calendarId, user.UserId);

        var response = await Api.GetAsync($"/api/private-class-sessions?calendarId={calendarId}");

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        var sessions = json!.AsArray();
        Assert.That(sessions.Any(x => x?["privateClassSessionId"]?.GetValue<int>() == sessionId), Is.True);
    }

    [Test]
    public async Task GetSessionById_ShouldReturnSession()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var sessionId = await CreateSessionAsync(calendarId, user.UserId);

        var response = await Api.GetAsync($"/api/private-class-sessions/{sessionId}");

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonInt(json, "privateClassSessionId"), Is.EqualTo(sessionId));
    }

    [Test]
    public async Task GetUnpaidSessions_ShouldReturnOnlyUnpaid()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var unpaidId = await CreateSessionAsync(calendarId, user.UserId, isPaid: false);
        await CreateSessionAsync(calendarId, user.UserId, isPaid: true);

        var response = await Api.GetAsync($"/api/private-class-sessions/unpaid?calendarId={calendarId}");

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        var sessions = json!.AsArray();
        Assert.That(sessions.Any(x => x?["privateClassSessionId"]?.GetValue<int>() == unpaidId), Is.True);
        Assert.That(sessions.All(x => x?["isPaid"]?.GetValue<bool>() == false), Is.True);
    }

    [Test]
    public async Task GetMonthlySummary_ShouldReturnCurrentMonthAggregation()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        await CreateSessionAsync(calendarId, user.UserId, isPaid: false);
        await CreateSessionAsync(calendarId, user.UserId, isPaid: true);

        var now = DateTime.UtcNow;
        var response = await Api.GetAsync($"/api/private-class-sessions/monthly-summary?calendarId={calendarId}&year={now.Year}&month={now.Month}");

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonInt(json, "calendarId"), Is.EqualTo(calendarId));
        Assert.That(JsonInt(json, "year"), Is.EqualTo(now.Year));
        Assert.That(JsonInt(json, "month"), Is.EqualTo(now.Month));
        Assert.That(JsonInt(json, "totalSessions"), Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public async Task CreateSession_ShouldCreateSession()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var student = Unique("session-create");

        var response = await Api.PostAsync("/api/private-class-sessions", new APIRequestContextOptions
        {
            DataObject = new
            {
                calendarId,
                createdByUserId = user.UserId,
                studentName = student,
                studentContact = "created@example.com",
                sessionStartUtc = FutureUtc(8).ToString("O"),
                sessionEndUtc = FutureUtc(9).ToString("O"),
                topicPlanned = "Topic",
                topicDone = (string?)null,
                homeworkAssigned = "Homework",
                priceAmount = 2500.00m,
                currencyCode = "RSD",
                isPaid = false,
                paidAtUtc = (string?)null,
                paymentMethod = (string?)null,
                paymentNote = (string?)null,
                status = "Scheduled"
            }
        });

        Assert.That(response.Status, Is.EqualTo(201));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonString(json, "studentName"), Is.EqualTo(student));
    }

    [Test]
    public async Task UpdateSession_ShouldModifySession()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var sessionId = await CreateSessionAsync(calendarId, user.UserId);
        var student = Unique("session-update");

        var response = await Api.PutAsync($"/api/private-class-sessions/{sessionId}", new APIRequestContextOptions
        {
            DataObject = new
            {
                calendarId,
                createdByUserId = user.UserId,
                studentName = student,
                studentContact = "updated@example.com",
                sessionStartUtc = FutureUtc(10).ToString("O"),
                sessionEndUtc = FutureUtc(11).ToString("O"),
                topicPlanned = "Updated topic",
                topicDone = "Done",
                homeworkAssigned = "Updated homework",
                priceAmount = 3200.00m,
                currencyCode = "RSD",
                isPaid = true,
                paidAtUtc = FutureUtc(1).ToString("O"),
                paymentMethod = "Card",
                paymentNote = "Paid by card",
                status = "Completed"
            }
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonString(json, "studentName"), Is.EqualTo(student));
        Assert.That(json!["isPaid"]!.GetValue<bool>(), Is.True);
    }

    [Test]
    public async Task MarkPaid_ShouldSetPaymentFields()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var sessionId = await CreateSessionAsync(calendarId, user.UserId, isPaid: false);

        var response = await Api.PutAsync($"/api/private-class-sessions/{sessionId}/mark-paid", new APIRequestContextOptions
        {
            DataObject = new
            {
                paymentMethod = "Transfer",
                paymentNote = "Paid by transfer",
                paidAtUtc = FutureUtc(1).ToString("O")
            }
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(json!["isPaid"]!.GetValue<bool>(), Is.True);
        Assert.That(JsonString(json, "paymentMethod"), Is.EqualTo("Transfer"));
    }

    [Test]
    public async Task MarkUnpaid_ShouldClearPaymentFields()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var sessionId = await CreateSessionAsync(calendarId, user.UserId, isPaid: true);

        var response = await Api.PutAsync($"/api/private-class-sessions/{sessionId}/mark-unpaid", new APIRequestContextOptions
        {
            DataObject = new { }
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(json!["isPaid"]!.GetValue<bool>(), Is.False);
        Assert.That(json!["paymentMethod"] is null || json["paymentMethod"]!.GetValue<string>() == string.Empty, Is.True);
    }

    [Test]
    public async Task DeleteSession_ShouldRemoveSession()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var sessionId = await CreateSessionAsync(calendarId, user.UserId);

        var deleteResponse = await Api.DeleteAsync($"/api/private-class-sessions/{sessionId}");
        Assert.That(deleteResponse.Status, Is.EqualTo(204));

        var readResponse = await Api.GetAsync($"/api/private-class-sessions/{sessionId}");
        Assert.That(readResponse.Status, Is.EqualTo(404));
    }
}
