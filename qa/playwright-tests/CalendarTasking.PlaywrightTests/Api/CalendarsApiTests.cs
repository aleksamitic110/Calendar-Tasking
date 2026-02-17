using CalendarTasking.PlaywrightTests.Infrastructure;
using Microsoft.Playwright;

namespace CalendarTasking.PlaywrightTests.Api;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class CalendarsApiTests : PlaywrightApiTestBase
{
    [Test]
    public async Task GetCalendars_ShouldReturnOwnerCalendars()
    {
        var owner = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(owner.UserId);

        var response = await Api.GetAsync($"/api/calendars?ownerUserId={owner.UserId}");

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        var calendars = json!.AsArray();
        Assert.That(calendars.Any(x => x?["calendarId"]?.GetValue<int>() == calendarId), Is.True);
    }

    [Test]
    public async Task GetCalendarById_ShouldReturnCalendar()
    {
        var owner = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(owner.UserId);

        var response = await Api.GetAsync($"/api/calendars/{calendarId}");

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonInt(json, "calendarId"), Is.EqualTo(calendarId));
    }

    [Test]
    public async Task CreateCalendar_ShouldCreateCalendar()
    {
        var owner = await RegisterUserAsync();
        var name = Unique("cal-create");

        var response = await Api.PostAsync("/api/calendars", new APIRequestContextOptions
        {
            DataObject = new
            {
                ownerUserId = owner.UserId,
                name,
                description = "Created by API test",
                colorHex = "#0F766E",
                isDefault = false
            }
        });

        Assert.That(response.Status, Is.EqualTo(201));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonString(json, "name"), Is.EqualTo(name));
    }

    [Test]
    public async Task UpdateCalendar_ShouldModifyCalendar()
    {
        var owner = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(owner.UserId);
        var newName = Unique("cal-update");

        var response = await Api.PutAsync($"/api/calendars/{calendarId}", new APIRequestContextOptions
        {
            DataObject = new
            {
                ownerUserId = owner.UserId,
                name = newName,
                description = "Updated",
                colorHex = "#0033AA",
                isDefault = false
            }
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonString(json, "name"), Is.EqualTo(newName));
    }

    [Test]
    public async Task DeleteCalendar_ShouldRemoveCalendar()
    {
        var owner = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(owner.UserId);

        var deleteResponse = await Api.DeleteAsync($"/api/calendars/{calendarId}");
        Assert.That(deleteResponse.Status, Is.EqualTo(204));

        var readResponse = await Api.GetAsync($"/api/calendars/{calendarId}");
        Assert.That(readResponse.Status, Is.EqualTo(404));
    }
}
