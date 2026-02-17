using CalendarTasking.PlaywrightTests.Infrastructure;
using Microsoft.Playwright;

namespace CalendarTasking.PlaywrightTests.Api;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class EventsApiTests : PlaywrightApiTestBase
{
    [Test]
    public async Task GetEvents_ShouldReturnCalendarEvents()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var eventId = await CreateEventAsync(calendarId, user.UserId);

        var response = await Api.GetAsync($"/api/events?calendarId={calendarId}");

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        var events = json!.AsArray();
        Assert.That(events.Any(x => x?["eventId"]?.GetValue<int>() == eventId), Is.True);
    }

    [Test]
    public async Task GetEventById_ShouldReturnEvent()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var eventId = await CreateEventAsync(calendarId, user.UserId);

        var response = await Api.GetAsync($"/api/events/{eventId}");

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonInt(json, "eventId"), Is.EqualTo(eventId));
    }

    [Test]
    public async Task CreateEvent_ShouldCreateEvent()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var start = FutureUtc(6);
        var end = FutureUtc(7);
        var title = Unique("event-create");

        var response = await Api.PostAsync("/api/events", new APIRequestContextOptions
        {
            DataObject = new
            {
                calendarId,
                createdByUserId = user.UserId,
                title,
                description = "Created by API test",
                location = "Room 3",
                startUtc = start.ToString("O"),
                endUtc = end.ToString("O"),
                isAllDay = false,
                repeatType = "None",
                reminderMinutesBefore = 15,
                status = "Planned"
            }
        });

        Assert.That(response.Status, Is.EqualTo(201));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonString(json, "title"), Is.EqualTo(title));
    }

    [Test]
    public async Task UpdateEvent_ShouldModifyEvent()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var eventId = await CreateEventAsync(calendarId, user.UserId);
        var start = FutureUtc(8);
        var end = FutureUtc(9);
        var title = Unique("event-update");

        var response = await Api.PutAsync($"/api/events/{eventId}", new APIRequestContextOptions
        {
            DataObject = new
            {
                calendarId,
                createdByUserId = user.UserId,
                title,
                description = "Updated by API test",
                location = "Online",
                startUtc = start.ToString("O"),
                endUtc = end.ToString("O"),
                isAllDay = false,
                repeatType = "Weekly",
                reminderMinutesBefore = 30,
                status = "Planned"
            }
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonString(json, "title"), Is.EqualTo(title));
        Assert.That(JsonString(json, "repeatType"), Is.EqualTo("Weekly"));
    }

    [Test]
    public async Task DeleteEvent_ShouldRemoveEvent()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var eventId = await CreateEventAsync(calendarId, user.UserId);

        var deleteResponse = await Api.DeleteAsync($"/api/events/{eventId}");
        Assert.That(deleteResponse.Status, Is.EqualTo(204));

        var readResponse = await Api.GetAsync($"/api/events/{eventId}");
        Assert.That(readResponse.Status, Is.EqualTo(404));
    }
}
