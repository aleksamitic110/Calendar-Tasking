using System.Net;
using System.Net.Http.Json;
using CalendarTasking.ComponentTests.Infrastructure;

namespace CalendarTasking.ComponentTests.Templates;

public sealed class CalendarsCrudTemplateTests : ComponentTestBase
{
    [Test]
    public async Task Create_ShouldReturnCreated_WhenPayloadIsValid_Template01()
    {
        var owner = await Client.RegisterUserAsync();
        var request = new CreateCalendarRequestDto(owner.UserId, $"Cal-{Guid.NewGuid():N}", "A", "#157A6E", false);

        var response = await Client.PostAsJsonAsync("/api/calendars", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await response.Content.ReadFromJsonAsync<CalendarResponseDto>();
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.OwnerUserId, Is.EqualTo(owner.UserId));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenOwnerDoesNotExist_Template02()
    {
        var request = new CreateCalendarRequestDto(999999, $"Cal-{Guid.NewGuid():N}", "A", "#157A6E", false);

        var response = await Client.PostAsJsonAsync("/api/calendars", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Create_ShouldReturnConflict_WhenNameIsDuplicateForOwner_Template03()
    {
        var owner = await Client.RegisterUserAsync();
        var name = $"Duplicate-{Guid.NewGuid():N}";
        await Client.CreateCalendarAsync(owner.UserId, name);

        var duplicateRequest = new CreateCalendarRequestDto(owner.UserId, $" {name} ", "Second", "#157A6E", false);
        var response = await Client.PostAsJsonAsync("/api/calendars", duplicateRequest);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task ReadAll_ShouldReturnOkAndCalendars_ForOwner_Template01()
    {
        var owner = await Client.RegisterUserAsync();
        var created = await Client.CreateCalendarAsync(owner.UserId);

        var response = await Client.GetAsync($"/api/calendars?ownerUserId={owner.UserId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var calendars = await response.Content.ReadFromJsonAsync<List<CalendarResponseDto>>();
        Assert.That(calendars, Is.Not.Null);
        Assert.That(calendars!.Select(x => x.CalendarId), Does.Contain(created.CalendarId));
    }

    [Test]
    public async Task ReadAll_ShouldRespectOwnerUserIdFilter_Template02()
    {
        var ownerOne = await Client.RegisterUserAsync();
        var ownerTwo = await Client.RegisterUserAsync();

        var firstOwnerCalendar = await Client.CreateCalendarAsync(ownerOne.UserId);
        await Client.CreateCalendarAsync(ownerOne.UserId);
        await Client.CreateCalendarAsync(ownerTwo.UserId);

        var response = await Client.GetAsync($"/api/calendars?ownerUserId={ownerOne.UserId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var calendars = await response.Content.ReadFromJsonAsync<List<CalendarResponseDto>>();
        Assert.That(calendars, Is.Not.Null);
        Assert.That(calendars!, Is.Not.Empty);
        Assert.That(calendars.All(x => x.OwnerUserId == ownerOne.UserId), Is.True);
        Assert.That(calendars.Select(x => x.CalendarId), Does.Contain(firstOwnerCalendar.CalendarId));
    }

    [Test]
    public async Task ReadAll_ShouldReturnOkAndEmptyList_WhenNoCalendarsExist_Template03()
    {
        var response = await Client.GetAsync("/api/calendars");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var calendars = await response.Content.ReadFromJsonAsync<List<CalendarResponseDto>>();
        Assert.That(calendars, Is.Not.Null);
        Assert.That(calendars!, Is.Empty);
    }

    [Test]
    public async Task ReadById_ShouldReturnOk_WhenCalendarExists_Template01()
    {
        var owner = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(owner.UserId);

        var response = await Client.GetAsync($"/api/calendars/{calendar.CalendarId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var found = await response.Content.ReadFromJsonAsync<CalendarResponseDto>();
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.CalendarId, Is.EqualTo(calendar.CalendarId));
    }

    [Test]
    public async Task ReadById_ShouldReturnNotFound_WhenCalendarDoesNotExist_Template02()
    {
        var response = await Client.GetAsync("/api/calendars/999999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ReadById_ShouldReturnExpectedCalendarFields_Template03()
    {
        var owner = await Client.RegisterUserAsync();
        var name = $"FieldCheck-{Guid.NewGuid():N}";
        var request = new CreateCalendarRequestDto(owner.UserId, name, "Desc", "#aa11cc", true);

        var createResponse = await Client.PostAsJsonAsync("/api/calendars", request);
        var created = await createResponse.Content.ReadFromJsonAsync<CalendarResponseDto>();
        Assert.That(created, Is.Not.Null);

        var response = await Client.GetAsync($"/api/calendars/{created!.CalendarId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var found = await response.Content.ReadFromJsonAsync<CalendarResponseDto>();
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.OwnerUserId, Is.EqualTo(owner.UserId));
        Assert.That(found.Name, Is.EqualTo(name));
        Assert.That(found.Description, Is.EqualTo("Desc"));
        Assert.That(found.ColorHex, Is.EqualTo("#AA11CC"));
        Assert.That(found.IsDefault, Is.True);
    }

    [Test]
    public async Task Update_ShouldReturnOk_WhenCalendarExistsAndPayloadIsValid_Template01()
    {
        var owner = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(owner.UserId);
        var request = new UpdateCalendarRequestDto(owner.UserId, "Updated calendar", "Updated", "#0033AA", false);

        var response = await Client.PutAsJsonAsync($"/api/calendars/{calendar.CalendarId}", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<CalendarResponseDto>();
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Name, Is.EqualTo("Updated calendar"));
        Assert.That(updated.ColorHex, Is.EqualTo("#0033AA"));
    }

    [Test]
    public async Task Update_ShouldReturnNotFound_WhenCalendarDoesNotExist_Template02()
    {
        var owner = await Client.RegisterUserAsync();
        var request = new UpdateCalendarRequestDto(owner.UserId, "Updated", "Updated", "#0033AA", false);

        var response = await Client.PutAsJsonAsync("/api/calendars/999999", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_ShouldReturnConflict_WhenNameIsDuplicateForOwner_Template03()
    {
        var owner = await Client.RegisterUserAsync();
        var first = await Client.CreateCalendarAsync(owner.UserId, $"Cal-A-{Guid.NewGuid():N}");
        var second = await Client.CreateCalendarAsync(owner.UserId, $"Cal-B-{Guid.NewGuid():N}");

        var conflictRequest = new UpdateCalendarRequestDto(owner.UserId, first.Name, "Try duplicate", "#0A0A0A", false);
        var response = await Client.PutAsJsonAsync($"/api/calendars/{second.CalendarId}", conflictRequest);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task Delete_ShouldReturnNoContent_WhenCalendarExists_Template01()
    {
        var owner = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(owner.UserId);

        var deleteResponse = await Client.DeleteAsync($"/api/calendars/{calendar.CalendarId}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getResponse = await Client.GetAsync($"/api/calendars/{calendar.CalendarId}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ShouldReturnNotFound_WhenCalendarDoesNotExist_Template02()
    {
        var response = await Client.DeleteAsync("/api/calendars/999999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ShouldRemoveCalendar_FromSubsequentReads_Template03()
    {
        var owner = await Client.RegisterUserAsync();
        var first = await Client.CreateCalendarAsync(owner.UserId);
        var second = await Client.CreateCalendarAsync(owner.UserId);

        var deleteResponse = await Client.DeleteAsync($"/api/calendars/{first.CalendarId}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var allResponse = await Client.GetAsync($"/api/calendars?ownerUserId={owner.UserId}");
        Assert.That(allResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var calendars = await allResponse.Content.ReadFromJsonAsync<List<CalendarResponseDto>>();
        Assert.That(calendars, Is.Not.Null);
        Assert.That(calendars!.Select(x => x.CalendarId), Does.Not.Contain(first.CalendarId));
        Assert.That(calendars.Select(x => x.CalendarId), Does.Contain(second.CalendarId));
    }
}
