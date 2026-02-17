using System.Net;
using System.Net.Http.Json;
using CalendarTasking.ComponentTests.Infrastructure;

namespace CalendarTasking.ComponentTests.Templates;

public sealed class PrivateClassSessionsCrudTemplateTests : ComponentTestBase
{
    [Test]
    public async Task Create_ShouldReturnCreated_WhenPayloadIsValid_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var request = new CreatePrivateClassSessionRequestDto(
            calendar.CalendarId,
            user.UserId,
            $"Student-{Guid.NewGuid():N}",
            "contact@example.com",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            "Plan",
            null,
            null,
            1500m,
            "RSD",
            false,
            null,
            null,
            null,
            "Scheduled");

        var response = await Client.PostAsJsonAsync("/api/private-class-sessions", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await response.Content.ReadFromJsonAsync<PrivateClassSessionResponseDto>();
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.CalendarId, Is.EqualTo(calendar.CalendarId));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenForeignKeysDoNotExist_Template02()
    {
        var request = new CreatePrivateClassSessionRequestDto(
            999999,
            999999,
            "Student",
            "contact@example.com",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            "Plan",
            null,
            null,
            1500m,
            "RSD",
            false,
            null,
            null,
            null,
            "Scheduled");

        var response = await Client.PostAsJsonAsync("/api/private-class-sessions", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenPayloadBreaksValidation_Template03()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);

        var request = new CreatePrivateClassSessionRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Student",
            "contact@example.com",
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(1),
            "Plan",
            null,
            null,
            1500m,
            "RSD",
            false,
            null,
            null,
            null,
            "Scheduled");

        var response = await Client.PostAsJsonAsync("/api/private-class-sessions", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ReadAll_ShouldReturnOkAndSessions_ForCalendar_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreatePrivateSessionAsync(calendar.CalendarId, user.UserId);

        var response = await Client.GetAsync($"/api/private-class-sessions?calendarId={calendar.CalendarId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var sessions = await response.Content.ReadFromJsonAsync<List<PrivateClassSessionResponseDto>>();
        Assert.That(sessions, Is.Not.Null);
        Assert.That(sessions!.Select(x => x.PrivateClassSessionId), Does.Contain(created.PrivateClassSessionId));
    }

    [Test]
    public async Task ReadAll_ShouldRespectCalendarIdAndPaidFilter_Template02()
    {
        var user = await Client.RegisterUserAsync();
        var firstCalendar = await Client.CreateCalendarAsync(user.UserId);
        var secondCalendar = await Client.CreateCalendarAsync(user.UserId);

        await Client.CreatePrivateSessionAsync(firstCalendar.CalendarId, user.UserId, $"Unpaid-{Guid.NewGuid():N}");
        var paidRequest = new CreatePrivateClassSessionRequestDto(
            firstCalendar.CalendarId,
            user.UserId,
            $"Paid-{Guid.NewGuid():N}",
            "contact@example.com",
            DateTime.UtcNow.AddHours(3),
            DateTime.UtcNow.AddHours(4),
            "Plan",
            null,
            null,
            2200m,
            "RSD",
            true,
            null,
            "Card",
            "Already paid",
            "Completed");
        var paidCreateResponse = await Client.PostAsJsonAsync("/api/private-class-sessions", paidRequest);
        var paidCreated = await paidCreateResponse.Content.ReadFromJsonAsync<PrivateClassSessionResponseDto>();
        Assert.That(paidCreated, Is.Not.Null);

        await Client.CreatePrivateSessionAsync(secondCalendar.CalendarId, user.UserId, $"OtherCal-{Guid.NewGuid():N}");

        var response = await Client.GetAsync($"/api/private-class-sessions?calendarId={firstCalendar.CalendarId}&isPaid=true");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var sessions = await response.Content.ReadFromJsonAsync<List<PrivateClassSessionResponseDto>>();
        Assert.That(sessions, Is.Not.Null);
        Assert.That(sessions!, Is.Not.Empty);
        Assert.That(sessions.All(x => x.CalendarId == firstCalendar.CalendarId), Is.True);
        Assert.That(sessions.All(x => x.IsPaid), Is.True);
        Assert.That(sessions.Select(x => x.PrivateClassSessionId), Does.Contain(paidCreated!.PrivateClassSessionId));
    }

    [Test]
    public async Task ReadAll_ShouldReturnOkAndEmptyList_WhenNoSessionsExist_Template03()
    {
        var response = await Client.GetAsync("/api/private-class-sessions");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var sessions = await response.Content.ReadFromJsonAsync<List<PrivateClassSessionResponseDto>>();
        Assert.That(sessions, Is.Not.Null);
        Assert.That(sessions!, Is.Empty);
    }

    [Test]
    public async Task ReadById_ShouldReturnOk_WhenSessionExists_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreatePrivateSessionAsync(calendar.CalendarId, user.UserId);

        var response = await Client.GetAsync($"/api/private-class-sessions/{created.PrivateClassSessionId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var found = await response.Content.ReadFromJsonAsync<PrivateClassSessionResponseDto>();
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.PrivateClassSessionId, Is.EqualTo(created.PrivateClassSessionId));
    }

    [Test]
    public async Task ReadById_ShouldReturnNotFound_WhenSessionDoesNotExist_Template02()
    {
        var response = await Client.GetAsync("/api/private-class-sessions/999999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ReadById_ShouldReturnExpectedSessionFields_Template03()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);

        var request = new CreatePrivateClassSessionRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Milica Student",
            "milica@example.com",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            "Functions",
            "Done topic",
            "Homework 1",
            1900m,
            "eur",
            true,
            null,
            "transfer",
            "Paid by bank",
            "Completed");

        var createResponse = await Client.PostAsJsonAsync("/api/private-class-sessions", request);
        var created = await createResponse.Content.ReadFromJsonAsync<PrivateClassSessionResponseDto>();
        Assert.That(created, Is.Not.Null);

        var response = await Client.GetAsync($"/api/private-class-sessions/{created!.PrivateClassSessionId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var found = await response.Content.ReadFromJsonAsync<PrivateClassSessionResponseDto>();
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.CalendarId, Is.EqualTo(calendar.CalendarId));
        Assert.That(found.CreatedByUserId, Is.EqualTo(user.UserId));
        Assert.That(found.StudentName, Is.EqualTo("Milica Student"));
        Assert.That(found.StudentContact, Is.EqualTo("milica@example.com"));
        Assert.That(found.PriceAmount, Is.EqualTo(1900m));
        Assert.That(found.CurrencyCode, Is.EqualTo("EUR"));
        Assert.That(found.IsPaid, Is.True);
        Assert.That(found.PaymentMethod, Is.EqualTo("Transfer"));
        Assert.That(found.Status, Is.EqualTo("Completed"));
    }

    [Test]
    public async Task Update_ShouldReturnOk_WhenSessionExistsAndPayloadIsValid_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreatePrivateSessionAsync(calendar.CalendarId, user.UserId);
        var request = new UpdatePrivateClassSessionRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Updated Student",
            "updated@example.com",
            DateTime.UtcNow.AddHours(3),
            DateTime.UtcNow.AddHours(4),
            "Updated plan",
            null,
            null,
            1700m,
            "RSD",
            false,
            null,
            null,
            "Updated note",
            "Scheduled");

        var response = await Client.PutAsJsonAsync($"/api/private-class-sessions/{created.PrivateClassSessionId}", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<PrivateClassSessionResponseDto>();
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.StudentName, Is.EqualTo("Updated Student"));
        Assert.That(updated.PriceAmount, Is.EqualTo(1700m));
    }

    [Test]
    public async Task Update_ShouldReturnNotFound_WhenSessionDoesNotExist_Template02()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var request = new UpdatePrivateClassSessionRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Missing",
            "updated@example.com",
            DateTime.UtcNow.AddHours(3),
            DateTime.UtcNow.AddHours(4),
            "Updated plan",
            null,
            null,
            1700m,
            "RSD",
            false,
            null,
            null,
            "Updated note",
            "Scheduled");

        var response = await Client.PutAsJsonAsync("/api/private-class-sessions/999999", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_ShouldReturnBadRequest_WhenPayloadBreaksValidation_Template03()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreatePrivateSessionAsync(calendar.CalendarId, user.UserId);

        var request = new UpdatePrivateClassSessionRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Invalid",
            "updated@example.com",
            DateTime.UtcNow.AddHours(4),
            DateTime.UtcNow.AddHours(3),
            "Updated plan",
            null,
            null,
            1700m,
            "RSD",
            false,
            null,
            null,
            "Updated note",
            "Scheduled");

        var response = await Client.PutAsJsonAsync($"/api/private-class-sessions/{created.PrivateClassSessionId}", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Delete_ShouldReturnNoContent_WhenSessionExists_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreatePrivateSessionAsync(calendar.CalendarId, user.UserId);

        var deleteResponse = await Client.DeleteAsync($"/api/private-class-sessions/{created.PrivateClassSessionId}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getResponse = await Client.GetAsync($"/api/private-class-sessions/{created.PrivateClassSessionId}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ShouldReturnNotFound_WhenSessionDoesNotExist_Template02()
    {
        var response = await Client.DeleteAsync("/api/private-class-sessions/999999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ShouldRemoveSession_FromSubsequentReads_Template03()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var first = await Client.CreatePrivateSessionAsync(calendar.CalendarId, user.UserId);
        var second = await Client.CreatePrivateSessionAsync(calendar.CalendarId, user.UserId);

        var deleteResponse = await Client.DeleteAsync($"/api/private-class-sessions/{first.PrivateClassSessionId}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var allResponse = await Client.GetAsync($"/api/private-class-sessions?calendarId={calendar.CalendarId}");
        Assert.That(allResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var sessions = await allResponse.Content.ReadFromJsonAsync<List<PrivateClassSessionResponseDto>>();
        Assert.That(sessions, Is.Not.Null);
        Assert.That(sessions!.Select(x => x.PrivateClassSessionId), Does.Not.Contain(first.PrivateClassSessionId));
        Assert.That(sessions.Select(x => x.PrivateClassSessionId), Does.Contain(second.PrivateClassSessionId));
    }
}
