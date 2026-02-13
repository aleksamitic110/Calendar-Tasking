using System.Net;
using System.Net.Http.Json;
using CalendarTasking.ComponentTests.Infrastructure;

namespace CalendarTasking.ComponentTests.Templates;

public sealed class CalendarsCrudTemplateTests : ComponentTestBase
{
    private const string TemplateIgnoreReason = "Template only. Implement test logic and remove [Ignore].";
    private const string TemplateFailMessage = "Template test. Add arrange/act/assert and remove [Ignore].";

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
    [Ignore(TemplateIgnoreReason)]
    public void Create_ShouldReturnBadRequest_WhenOwnerDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Create_ShouldReturnConflict_WhenNameIsDuplicateForOwner_Template03()
    {
        Assert.Fail(TemplateFailMessage);
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
    [Ignore(TemplateIgnoreReason)]
    public void ReadAll_ShouldRespectOwnerUserIdFilter_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadAll_ShouldReturnOkAndEmptyList_WhenNoCalendarsExist_Template03()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnOk_WhenCalendarExists_Template01()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnNotFound_WhenCalendarDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnExpectedCalendarFields_Template03()
    {
        Assert.Fail(TemplateFailMessage);
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
    [Ignore(TemplateIgnoreReason)]
    public void Update_ShouldReturnNotFound_WhenCalendarDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Update_ShouldReturnConflict_WhenNameIsDuplicateForOwner_Template03()
    {
        Assert.Fail(TemplateFailMessage);
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
    [Ignore(TemplateIgnoreReason)]
    public void Delete_ShouldReturnNotFound_WhenCalendarDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Delete_ShouldRemoveCalendar_FromSubsequentReads_Template03()
    {
        Assert.Fail(TemplateFailMessage);
    }
}
