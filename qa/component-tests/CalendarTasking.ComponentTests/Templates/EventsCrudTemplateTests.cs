using System.Net;
using System.Net.Http.Json;
using CalendarTasking.ComponentTests.Infrastructure;

namespace CalendarTasking.ComponentTests.Templates;

public sealed class EventsCrudTemplateTests : ComponentTestBase
{
    private const string TemplateIgnoreReason = "Template only. Implement test logic and remove [Ignore].";
    private const string TemplateFailMessage = "Template test. Add arrange/act/assert and remove [Ignore].";

    [Test]
    public async Task Create_ShouldReturnCreated_WhenPayloadIsValid_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var request = new CreateEventRequestDto(
            calendar.CalendarId,
            user.UserId,
            $"Event-{Guid.NewGuid():N}",
            "Desc",
            "Home",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            false,
            "None",
            5,
            "Planned");

        var response = await Client.PostAsJsonAsync("/api/events", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await response.Content.ReadFromJsonAsync<EventResponseDto>();
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
    public void Create_ShouldReturnBadRequest_WhenEndIsBeforeStart_Template03()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    public async Task ReadAll_ShouldReturnOkAndEvents_ForCalendar_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreateEventAsync(calendar.CalendarId, user.UserId);

        var response = await Client.GetAsync($"/api/events?calendarId={calendar.CalendarId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var events = await response.Content.ReadFromJsonAsync<List<EventResponseDto>>();
        Assert.That(events, Is.Not.Null);
        Assert.That(events!.Select(x => x.EventId), Does.Contain(created.EventId));
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadAll_ShouldRespectCalendarIdFilter_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadAll_ShouldReturnOkAndEmptyList_WhenNoEventsExist_Template03()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnOk_WhenEventExists_Template01()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnNotFound_WhenEventDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnExpectedEventFields_Template03()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    public async Task Update_ShouldReturnOk_WhenEventExistsAndPayloadIsValid_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreateEventAsync(calendar.CalendarId, user.UserId);
        var request = new UpdateEventRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Updated event title",
            "Updated desc",
            "Office",
            DateTime.UtcNow.AddHours(3),
            DateTime.UtcNow.AddHours(4),
            false,
            "None",
            15,
            "Planned");

        var response = await Client.PutAsJsonAsync($"/api/events/{created.EventId}", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<EventResponseDto>();
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Title, Is.EqualTo("Updated event title"));
        Assert.That(updated.Location, Is.EqualTo("Office"));
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Update_ShouldReturnNotFound_WhenEventDoesNotExist_Template02()
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
    public async Task Delete_ShouldReturnNoContent_WhenEventExists_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreateEventAsync(calendar.CalendarId, user.UserId);

        var deleteResponse = await Client.DeleteAsync($"/api/events/{created.EventId}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getResponse = await Client.GetAsync($"/api/events/{created.EventId}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Delete_ShouldReturnNotFound_WhenEventDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Delete_ShouldRemoveEvent_FromSubsequentReads_Template03()
    {
        Assert.Fail(TemplateFailMessage);
    }
}
