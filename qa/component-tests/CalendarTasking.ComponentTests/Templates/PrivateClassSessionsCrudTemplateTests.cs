using System.Net;
using System.Net.Http.Json;
using CalendarTasking.ComponentTests.Infrastructure;

namespace CalendarTasking.ComponentTests.Templates;

public sealed class PrivateClassSessionsCrudTemplateTests : ComponentTestBase
{
    private const string TemplateIgnoreReason = "Template only. Implement test logic and remove [Ignore].";
    private const string TemplateFailMessage = "Template test. Add arrange/act/assert and remove [Ignore].";

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
    [Ignore(TemplateIgnoreReason)]
    public void Create_ShouldReturnBadRequest_WhenForeignKeysDoNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Create_ShouldReturnBadRequest_WhenPayloadBreaksValidation_Template03()
    {
        Assert.Fail(TemplateFailMessage);
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
    [Ignore(TemplateIgnoreReason)]
    public void ReadAll_ShouldRespectCalendarIdAndPaidFilter_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadAll_ShouldReturnOkAndEmptyList_WhenNoSessionsExist_Template03()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnOk_WhenSessionExists_Template01()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnNotFound_WhenSessionDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnExpectedSessionFields_Template03()
    {
        Assert.Fail(TemplateFailMessage);
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
    [Ignore(TemplateIgnoreReason)]
    public void Update_ShouldReturnNotFound_WhenSessionDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Update_ShouldReturnBadRequest_WhenPayloadBreaksValidation_Template03()
    {
        Assert.Fail(TemplateFailMessage);
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
    [Ignore(TemplateIgnoreReason)]
    public void Delete_ShouldReturnNotFound_WhenSessionDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Delete_ShouldRemoveSession_FromSubsequentReads_Template03()
    {
        Assert.Fail(TemplateFailMessage);
    }
}
