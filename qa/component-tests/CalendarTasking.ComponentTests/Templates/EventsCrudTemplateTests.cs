using System.Net;
using System.Net.Http.Json;
using CalendarTasking.ComponentTests.Infrastructure;

namespace CalendarTasking.ComponentTests.Templates;

public sealed class EventsCrudTemplateTests : ComponentTestBase
{
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
    public async Task Create_ShouldReturnBadRequest_WhenForeignKeysDoNotExist_Template02()
    {
        var now = DateTime.UtcNow;
        var request = new CreateEventRequestDto(
            999999,
            999999,
            "Invalid FK",
            "Desc",
            "Home",
            now.AddHours(1),
            now.AddHours(2),
            false,
            "None",
            5,
            "Planned");

        var response = await Client.PostAsJsonAsync("/api/events", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenEndIsBeforeStart_Template03()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var now = DateTime.UtcNow;

        var request = new CreateEventRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Invalid time range",
            "Desc",
            "Home",
            now.AddHours(2),
            now.AddHours(1),
            false,
            "None",
            5,
            "Planned");

        var response = await Client.PostAsJsonAsync("/api/events", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
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
    public async Task ReadAll_ShouldRespectCalendarIdFilter_Template02()
    {
        var user = await Client.RegisterUserAsync();
        var firstCalendar = await Client.CreateCalendarAsync(user.UserId);
        var secondCalendar = await Client.CreateCalendarAsync(user.UserId);

        var firstEvent = await Client.CreateEventAsync(firstCalendar.CalendarId, user.UserId);
        await Client.CreateEventAsync(secondCalendar.CalendarId, user.UserId);

        var response = await Client.GetAsync($"/api/events?calendarId={firstCalendar.CalendarId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var events = await response.Content.ReadFromJsonAsync<List<EventResponseDto>>();
        Assert.That(events, Is.Not.Null);
        Assert.That(events!, Is.Not.Empty);
        Assert.That(events.All(x => x.CalendarId == firstCalendar.CalendarId), Is.True);
        Assert.That(events.Select(x => x.EventId), Does.Contain(firstEvent.EventId));
    }

    [Test]
    public async Task ReadAll_ShouldReturnOkAndEmptyList_WhenNoEventsExist_Template03()
    {
        var response = await Client.GetAsync("/api/events");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var events = await response.Content.ReadFromJsonAsync<List<EventResponseDto>>();
        Assert.That(events, Is.Not.Null);
        Assert.That(events!, Is.Empty);
    }

    [Test]
    public async Task ReadById_ShouldReturnOk_WhenEventExists_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreateEventAsync(calendar.CalendarId, user.UserId);

        var response = await Client.GetAsync($"/api/events/{created.EventId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var found = await response.Content.ReadFromJsonAsync<EventResponseDto>();
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.EventId, Is.EqualTo(created.EventId));
    }

    [Test]
    public async Task ReadById_ShouldReturnNotFound_WhenEventDoesNotExist_Template02()
    {
        var response = await Client.GetAsync("/api/events/999999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ReadById_ShouldReturnExpectedEventFields_Template03()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(2);

        var request = new CreateEventRequestDto(
            calendar.CalendarId,
            user.UserId,
            "ReadById Event",
            "Detailed description",
            "Classroom",
            start,
            end,
            false,
            "Weekly",
            30,
            "Planned");

        var createResponse = await Client.PostAsJsonAsync("/api/events", request);
        var created = await createResponse.Content.ReadFromJsonAsync<EventResponseDto>();
        Assert.That(created, Is.Not.Null);

        var response = await Client.GetAsync($"/api/events/{created!.EventId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var found = await response.Content.ReadFromJsonAsync<EventResponseDto>();
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.CalendarId, Is.EqualTo(calendar.CalendarId));
        Assert.That(found.CreatedByUserId, Is.EqualTo(user.UserId));
        Assert.That(found.Title, Is.EqualTo("ReadById Event"));
        Assert.That(found.Description, Is.EqualTo("Detailed description"));
        Assert.That(found.Location, Is.EqualTo("Classroom"));
        Assert.That(found.RepeatType, Is.EqualTo("Weekly"));
        Assert.That(found.ReminderMinutesBefore, Is.EqualTo(30));
        Assert.That(found.Status, Is.EqualTo("Planned"));
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
    public async Task Update_ShouldReturnNotFound_WhenEventDoesNotExist_Template02()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var request = new UpdateEventRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Missing",
            "Desc",
            "Office",
            DateTime.UtcNow.AddHours(3),
            DateTime.UtcNow.AddHours(4),
            false,
            "None",
            15,
            "Planned");

        var response = await Client.PutAsJsonAsync("/api/events/999999", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_ShouldReturnBadRequest_WhenPayloadBreaksValidation_Template03()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreateEventAsync(calendar.CalendarId, user.UserId);

        var request = new UpdateEventRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Invalid",
            "Desc",
            "Office",
            DateTime.UtcNow.AddHours(4),
            DateTime.UtcNow.AddHours(3),
            false,
            "None",
            15,
            "Planned");

        var response = await Client.PutAsJsonAsync($"/api/events/{created.EventId}", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
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
    public async Task Delete_ShouldReturnNotFound_WhenEventDoesNotExist_Template02()
    {
        var response = await Client.DeleteAsync("/api/events/999999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ShouldRemoveEvent_FromSubsequentReads_Template03()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var first = await Client.CreateEventAsync(calendar.CalendarId, user.UserId);
        var second = await Client.CreateEventAsync(calendar.CalendarId, user.UserId);

        var deleteResponse = await Client.DeleteAsync($"/api/events/{first.EventId}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var allResponse = await Client.GetAsync($"/api/events?calendarId={calendar.CalendarId}");
        Assert.That(allResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var events = await allResponse.Content.ReadFromJsonAsync<List<EventResponseDto>>();
        Assert.That(events, Is.Not.Null);
        Assert.That(events!.Select(x => x.EventId), Does.Not.Contain(first.EventId));
        Assert.That(events.Select(x => x.EventId), Does.Contain(second.EventId));
    }
}
