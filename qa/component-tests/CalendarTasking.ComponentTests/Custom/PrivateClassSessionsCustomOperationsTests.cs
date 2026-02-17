using System.Net;
using System.Net.Http.Json;
using CalendarTasking.ComponentTests.Infrastructure;

namespace CalendarTasking.ComponentTests.Custom;

public sealed class PrivateClassSessionsCustomOperationsTests : ComponentTestBase
{
    [Test]
    public async Task GetUnpaid_ShouldReturnOnlyUnpaidSessions()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var unpaid = await Client.CreatePrivateSessionAsync(calendar.CalendarId, user.UserId);

        var paidRequest = new CreatePrivateClassSessionRequestDto(
            calendar.CalendarId,
            user.UserId,
            $"Paid-{Guid.NewGuid():N}",
            "student@example.com",
            DateTime.UtcNow.AddHours(3),
            DateTime.UtcNow.AddHours(4),
            "Topic",
            null,
            null,
            2000m,
            "RSD",
            true,
            null,
            "Card",
            "Paid",
            "Completed");
        var paidCreateResponse = await Client.PostAsJsonAsync("/api/private-class-sessions", paidRequest);
        var paid = await paidCreateResponse.Content.ReadFromJsonAsync<PrivateClassSessionResponseDto>();
        Assert.That(paid, Is.Not.Null);

        var response = await Client.GetAsync($"/api/private-class-sessions/unpaid?calendarId={calendar.CalendarId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var unpaidSessions = await response.Content.ReadFromJsonAsync<List<PrivateClassSessionResponseDto>>();
        Assert.That(unpaidSessions, Is.Not.Null);
        Assert.That(unpaidSessions!, Is.Not.Empty);
        Assert.That(unpaidSessions.All(x => !x.IsPaid), Is.True);
        Assert.That(unpaidSessions.Select(x => x.PrivateClassSessionId), Does.Contain(unpaid.PrivateClassSessionId));
        Assert.That(unpaidSessions.Select(x => x.PrivateClassSessionId), Does.Not.Contain(paid!.PrivateClassSessionId));
    }

    [Test]
    public async Task GetUnpaid_ShouldRespectCalendarIdFilter()
    {
        var user = await Client.RegisterUserAsync();
        var firstCalendar = await Client.CreateCalendarAsync(user.UserId);
        var secondCalendar = await Client.CreateCalendarAsync(user.UserId);

        var firstCalendarSession = await Client.CreatePrivateSessionAsync(firstCalendar.CalendarId, user.UserId);
        var secondCalendarSession = await Client.CreatePrivateSessionAsync(secondCalendar.CalendarId, user.UserId);

        var response = await Client.GetAsync($"/api/private-class-sessions/unpaid?calendarId={firstCalendar.CalendarId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var sessions = await response.Content.ReadFromJsonAsync<List<PrivateClassSessionResponseDto>>();
        Assert.That(sessions, Is.Not.Null);
        Assert.That(sessions!, Is.Not.Empty);
        Assert.That(sessions.All(x => x.CalendarId == firstCalendar.CalendarId), Is.True);
        Assert.That(sessions.Select(x => x.PrivateClassSessionId), Does.Contain(firstCalendarSession.PrivateClassSessionId));
        Assert.That(sessions.Select(x => x.PrivateClassSessionId), Does.Not.Contain(secondCalendarSession.PrivateClassSessionId));
    }

    [Test]
    public async Task GetUnpaid_ShouldReturnEmptyList_WhenAllSessionsArePaid()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);

        var paidRequest = new CreatePrivateClassSessionRequestDto(
            calendar.CalendarId,
            user.UserId,
            $"Paid-{Guid.NewGuid():N}",
            "student@example.com",
            DateTime.UtcNow.AddHours(3),
            DateTime.UtcNow.AddHours(4),
            "Topic",
            null,
            null,
            2000m,
            "RSD",
            true,
            null,
            "Card",
            "Paid",
            "Completed");
        var createResponse = await Client.PostAsJsonAsync("/api/private-class-sessions", paidRequest);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var response = await Client.GetAsync($"/api/private-class-sessions/unpaid?calendarId={calendar.CalendarId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var sessions = await response.Content.ReadFromJsonAsync<List<PrivateClassSessionResponseDto>>();
        Assert.That(sessions, Is.Not.Null);
        Assert.That(sessions!, Is.Empty);
    }

    [Test]
    public async Task MonthlySummary_ShouldReturnAggregatedTotals_ForRequestedMonth()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        const int year = 2026;
        const int month = 1;

        var paidInMonth = new CreatePrivateClassSessionRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Student Paid",
            "paid@example.com",
            new DateTime(year, month, 10, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(year, month, 10, 11, 0, 0, DateTimeKind.Utc),
            "Topic",
            null,
            null,
            1200m,
            "RSD",
            true,
            null,
            "Card",
            "Paid in month",
            "Completed");

        var unpaidInMonth = new CreatePrivateClassSessionRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Student Unpaid",
            "unpaid@example.com",
            new DateTime(year, month, 12, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(year, month, 12, 11, 0, 0, DateTimeKind.Utc),
            "Topic",
            null,
            null,
            1800m,
            "RSD",
            false,
            null,
            null,
            null,
            "Scheduled");

        var paidOtherMonth = new CreatePrivateClassSessionRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Student Other Month",
            "other@example.com",
            new DateTime(year, 2, 5, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(year, 2, 5, 11, 0, 0, DateTimeKind.Utc),
            "Topic",
            null,
            null,
            3000m,
            "RSD",
            true,
            null,
            "Transfer",
            "Paid in another month",
            "Completed");

        Assert.That((await Client.PostAsJsonAsync("/api/private-class-sessions", paidInMonth)).StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That((await Client.PostAsJsonAsync("/api/private-class-sessions", unpaidInMonth)).StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That((await Client.PostAsJsonAsync("/api/private-class-sessions", paidOtherMonth)).StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var response = await Client.GetAsync($"/api/private-class-sessions/monthly-summary?calendarId={calendar.CalendarId}&year={year}&month={month}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var summary = await response.Content.ReadFromJsonAsync<PrivateClassMonthlySummaryResponseTestDto>();
        Assert.That(summary, Is.Not.Null);
        Assert.That(summary!.CalendarId, Is.EqualTo(calendar.CalendarId));
        Assert.That(summary.Year, Is.EqualTo(year));
        Assert.That(summary.Month, Is.EqualTo(month));
        Assert.That(summary.TotalSessions, Is.EqualTo(2));
        Assert.That(summary.PaidSessions, Is.EqualTo(1));
        Assert.That(summary.UnpaidSessions, Is.EqualTo(1));
        Assert.That(summary.TotalPaidAmount, Is.EqualTo(1200m));
        Assert.That(summary.TotalScheduledAmount, Is.EqualTo(3000m));
    }

    [Test]
    public async Task MonthlySummary_ShouldReturnBadRequest_WhenYearIsOutOfRange()
    {
        var response = await Client.GetAsync("/api/private-class-sessions/monthly-summary?calendarId=1&year=1999&month=1");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task MonthlySummary_ShouldReturnBadRequest_WhenMonthIsOutOfRange()
    {
        var response = await Client.GetAsync("/api/private-class-sessions/monthly-summary?calendarId=1&year=2026&month=13");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task MarkPaid_ShouldReturnOkAndSetPaymentFields_WhenRequestIsValid()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var session = await Client.CreatePrivateSessionAsync(calendar.CalendarId, user.UserId);

        var response = await Client.PutAsJsonAsync(
            $"/api/private-class-sessions/{session.PrivateClassSessionId}/mark-paid",
            new MarkSessionPaidRequestTestDto("cash", "Paid in cash", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<PrivateClassSessionResponseDto>();
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.IsPaid, Is.True);
        Assert.That(updated.PaidAtUtc, Is.Not.Null);
        Assert.That(updated.PaymentMethod, Is.EqualTo("Cash"));
        Assert.That(updated.PaymentNote, Is.EqualTo("Paid in cash"));
    }

    [Test]
    public async Task MarkPaid_ShouldReturnBadRequest_WhenPaymentMethodIsInvalid()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var session = await Client.CreatePrivateSessionAsync(calendar.CalendarId, user.UserId);

        var response = await Client.PutAsJsonAsync(
            $"/api/private-class-sessions/{session.PrivateClassSessionId}/mark-paid",
            new MarkSessionPaidRequestTestDto("Crypto", "Invalid method", DateTime.UtcNow));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task MarkPaid_ShouldReturnNotFound_WhenSessionDoesNotExist()
    {
        var response = await Client.PutAsJsonAsync(
            "/api/private-class-sessions/999999/mark-paid",
            new MarkSessionPaidRequestTestDto("Card", "Missing session", DateTime.UtcNow));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task MarkUnpaid_ShouldReturnOkAndClearPaymentFields_WhenSessionExists()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);

        var paidRequest = new CreatePrivateClassSessionRequestDto(
            calendar.CalendarId,
            user.UserId,
            $"Paid-{Guid.NewGuid():N}",
            "student@example.com",
            DateTime.UtcNow.AddHours(3),
            DateTime.UtcNow.AddHours(4),
            "Topic",
            null,
            null,
            2000m,
            "RSD",
            true,
            DateTime.UtcNow,
            "Card",
            "Was paid",
            "Completed");
        var createResponse = await Client.PostAsJsonAsync("/api/private-class-sessions", paidRequest);
        var paidSession = await createResponse.Content.ReadFromJsonAsync<PrivateClassSessionResponseDto>();
        Assert.That(paidSession, Is.Not.Null);

        var response = await Client.PutAsync($"/api/private-class-sessions/{paidSession!.PrivateClassSessionId}/mark-unpaid", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<PrivateClassSessionResponseDto>();
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.IsPaid, Is.False);
        Assert.That(updated.PaidAtUtc, Is.Null);
        Assert.That(updated.PaymentMethod, Is.Null);
        Assert.That(updated.PaymentNote, Is.Null);
    }

    [Test]
    public async Task MarkUnpaid_ShouldReturnNotFound_WhenSessionDoesNotExist()
    {
        var response = await Client.PutAsync("/api/private-class-sessions/999999/mark-unpaid", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task MarkUnpaid_ShouldRemainUnpaid_WhenSessionIsAlreadyUnpaid()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var session = await Client.CreatePrivateSessionAsync(calendar.CalendarId, user.UserId);

        var response = await Client.PutAsync($"/api/private-class-sessions/{session.PrivateClassSessionId}/mark-unpaid", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<PrivateClassSessionResponseDto>();
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.IsPaid, Is.False);
        Assert.That(updated.PaidAtUtc, Is.Null);
        Assert.That(updated.PaymentMethod, Is.Null);
        Assert.That(updated.PaymentNote, Is.Null);
    }
}

public sealed record MarkSessionPaidRequestTestDto(string? PaymentMethod, string? PaymentNote, DateTime? PaidAtUtc);

public sealed record PrivateClassMonthlySummaryResponseTestDto(
    int CalendarId,
    int Year,
    int Month,
    decimal TotalPaidAmount,
    decimal TotalScheduledAmount,
    int TotalSessions,
    int PaidSessions,
    int UnpaidSessions);
