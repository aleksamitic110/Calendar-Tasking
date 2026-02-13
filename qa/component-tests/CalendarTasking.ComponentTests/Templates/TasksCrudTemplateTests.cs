using System.Net;
using System.Net.Http.Json;
using CalendarTasking.ComponentTests.Infrastructure;

namespace CalendarTasking.ComponentTests.Templates;

public sealed class TasksCrudTemplateTests : ComponentTestBase
{
    private const string TemplateIgnoreReason = "Template only. Implement test logic and remove [Ignore].";
    private const string TemplateFailMessage = "Template test. Add arrange/act/assert and remove [Ignore].";

    [Test]
    public async Task Create_ShouldReturnCreated_WhenPayloadIsValid_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var request = new CreateTaskRequestDto(
            calendar.CalendarId,
            user.UserId,
            $"Task-{Guid.NewGuid():N}",
            "Task desc",
            DateTime.UtcNow.AddDays(2),
            "Medium",
            "Todo",
            null,
            10);

        var response = await Client.PostAsJsonAsync("/api/tasks", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
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
    public async Task ReadAll_ShouldReturnOkAndTasks_ForCalendar_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreateTaskAsync(calendar.CalendarId, user.UserId);

        var response = await Client.GetAsync($"/api/tasks?calendarId={calendar.CalendarId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
        Assert.That(tasks, Is.Not.Null);
        Assert.That(tasks!.Select(x => x.TaskItemId), Does.Contain(created.TaskItemId));
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadAll_ShouldRespectCalendarIdAndStatusFilters_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadAll_ShouldReturnOkAndEmptyList_WhenNoTasksExist_Template03()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnOk_WhenTaskExists_Template01()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnNotFound_WhenTaskDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnExpectedTaskFields_Template03()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    public async Task Update_ShouldReturnOk_WhenTaskExistsAndPayloadIsValid_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreateTaskAsync(calendar.CalendarId, user.UserId);
        var request = new UpdateTaskRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Updated task title",
            "Updated desc",
            DateTime.UtcNow.AddDays(3),
            "High",
            "InProgress",
            null,
            20);

        var response = await Client.PutAsJsonAsync($"/api/tasks/{created.TaskItemId}", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Title, Is.EqualTo("Updated task title"));
        Assert.That(updated.Priority, Is.EqualTo("High"));
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Update_ShouldReturnNotFound_WhenTaskDoesNotExist_Template02()
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
    public async Task Delete_ShouldReturnNoContent_WhenTaskExists_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreateTaskAsync(calendar.CalendarId, user.UserId);

        var deleteResponse = await Client.DeleteAsync($"/api/tasks/{created.TaskItemId}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getResponse = await Client.GetAsync($"/api/tasks/{created.TaskItemId}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Delete_ShouldReturnNotFound_WhenTaskDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Delete_ShouldRemoveTask_FromSubsequentReads_Template03()
    {
        Assert.Fail(TemplateFailMessage);
    }
}
