using System.Net;
using System.Net.Http.Json;
using CalendarTasking.ComponentTests.Infrastructure;

namespace CalendarTasking.ComponentTests.Templates;

public sealed class TasksCrudTemplateTests : ComponentTestBase
{
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
    public async Task Create_ShouldReturnBadRequest_WhenForeignKeysDoNotExist_Template02()
    {
        var request = new CreateTaskRequestDto(
            999999,
            999999,
            "Invalid FK",
            "Task desc",
            DateTime.UtcNow.AddDays(2),
            "Medium",
            "Todo",
            null,
            10);

        var response = await Client.PostAsJsonAsync("/api/tasks", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenPayloadBreaksValidation_Template03()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);

        var request = new CreateTaskRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Invalid payload",
            "Task desc",
            DateTime.UtcNow.AddDays(2),
            "Urgent",
            "Todo",
            null,
            10);

        var response = await Client.PostAsJsonAsync("/api/tasks", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
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
    public async Task ReadAll_ShouldRespectCalendarIdAndStatusFilters_Template02()
    {
        var user = await Client.RegisterUserAsync();
        var firstCalendar = await Client.CreateCalendarAsync(user.UserId);
        var secondCalendar = await Client.CreateCalendarAsync(user.UserId);

        var todoTask = await Client.CreateTaskAsync(firstCalendar.CalendarId, user.UserId);
        await Client.PostAsJsonAsync("/api/tasks", new CreateTaskRequestDto(
            firstCalendar.CalendarId,
            user.UserId,
            $"Done-{Guid.NewGuid():N}",
            "Done task",
            DateTime.UtcNow.AddDays(1),
            "Low",
            "Done",
            null,
            null));
        await Client.CreateTaskAsync(secondCalendar.CalendarId, user.UserId);

        var response = await Client.GetAsync($"/api/tasks?calendarId={firstCalendar.CalendarId}&status=todo");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
        Assert.That(tasks, Is.Not.Null);
        Assert.That(tasks!, Is.Not.Empty);
        Assert.That(tasks.All(x => x.CalendarId == firstCalendar.CalendarId), Is.True);
        Assert.That(tasks.All(x => x.Status == "Todo"), Is.True);
        Assert.That(tasks.Select(x => x.TaskItemId), Does.Contain(todoTask.TaskItemId));
    }

    [Test]
    public async Task ReadAll_ShouldReturnOkAndEmptyList_WhenNoTasksExist_Template03()
    {
        var response = await Client.GetAsync("/api/tasks");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
        Assert.That(tasks, Is.Not.Null);
        Assert.That(tasks!, Is.Empty);
    }

    [Test]
    public async Task ReadById_ShouldReturnOk_WhenTaskExists_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreateTaskAsync(calendar.CalendarId, user.UserId);

        var response = await Client.GetAsync($"/api/tasks/{created.TaskItemId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var found = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.TaskItemId, Is.EqualTo(created.TaskItemId));
    }

    [Test]
    public async Task ReadById_ShouldReturnNotFound_WhenTaskDoesNotExist_Template02()
    {
        var response = await Client.GetAsync("/api/tasks/999999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ReadById_ShouldReturnExpectedTaskFields_Template03()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);

        var request = new CreateTaskRequestDto(
            calendar.CalendarId,
            user.UserId,
            "ReadById Task",
            "Task details",
            DateTime.UtcNow.AddDays(4),
            "High",
            "InProgress",
            null,
            25);

        var createResponse = await Client.PostAsJsonAsync("/api/tasks", request);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.That(created, Is.Not.Null);

        var response = await Client.GetAsync($"/api/tasks/{created!.TaskItemId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var found = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.CalendarId, Is.EqualTo(calendar.CalendarId));
        Assert.That(found.CreatedByUserId, Is.EqualTo(user.UserId));
        Assert.That(found.Title, Is.EqualTo("ReadById Task"));
        Assert.That(found.Description, Is.EqualTo("Task details"));
        Assert.That(found.Priority, Is.EqualTo("High"));
        Assert.That(found.Status, Is.EqualTo("InProgress"));
        Assert.That(found.ReminderMinutesBefore, Is.EqualTo(25));
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
    public async Task Update_ShouldReturnNotFound_WhenTaskDoesNotExist_Template02()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);

        var request = new UpdateTaskRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Missing",
            "Updated desc",
            DateTime.UtcNow.AddDays(3),
            "High",
            "InProgress",
            null,
            20);

        var response = await Client.PutAsJsonAsync("/api/tasks/999999", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_ShouldReturnBadRequest_WhenPayloadBreaksValidation_Template03()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var created = await Client.CreateTaskAsync(calendar.CalendarId, user.UserId);

        var request = new UpdateTaskRequestDto(
            calendar.CalendarId,
            user.UserId,
            "Invalid",
            "Updated desc",
            DateTime.UtcNow.AddDays(3),
            "InvalidPriority",
            "InProgress",
            null,
            20);

        var response = await Client.PutAsJsonAsync($"/api/tasks/{created.TaskItemId}", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
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
    public async Task Delete_ShouldReturnNotFound_WhenTaskDoesNotExist_Template02()
    {
        var response = await Client.DeleteAsync("/api/tasks/999999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ShouldRemoveTask_FromSubsequentReads_Template03()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var first = await Client.CreateTaskAsync(calendar.CalendarId, user.UserId);
        var second = await Client.CreateTaskAsync(calendar.CalendarId, user.UserId);

        var deleteResponse = await Client.DeleteAsync($"/api/tasks/{first.TaskItemId}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var allResponse = await Client.GetAsync($"/api/tasks?calendarId={calendar.CalendarId}");
        Assert.That(allResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var tasks = await allResponse.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
        Assert.That(tasks, Is.Not.Null);
        Assert.That(tasks!.Select(x => x.TaskItemId), Does.Not.Contain(first.TaskItemId));
        Assert.That(tasks.Select(x => x.TaskItemId), Does.Contain(second.TaskItemId));
    }
}
