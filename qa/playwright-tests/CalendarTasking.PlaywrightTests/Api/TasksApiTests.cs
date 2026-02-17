using CalendarTasking.PlaywrightTests.Infrastructure;
using Microsoft.Playwright;

namespace CalendarTasking.PlaywrightTests.Api;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class TasksApiTests : PlaywrightApiTestBase
{
    [Test]
    public async Task GetTasks_ShouldReturnCalendarTasks()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var taskId = await CreateTaskAsync(calendarId, user.UserId);

        var response = await Api.GetAsync($"/api/tasks?calendarId={calendarId}");

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        var tasks = json!.AsArray();
        Assert.That(tasks.Any(x => x?["taskItemId"]?.GetValue<int>() == taskId), Is.True);
    }

    [Test]
    public async Task GetTaskById_ShouldReturnTask()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var taskId = await CreateTaskAsync(calendarId, user.UserId);

        var response = await Api.GetAsync($"/api/tasks/{taskId}");

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonInt(json, "taskItemId"), Is.EqualTo(taskId));
    }

    [Test]
    public async Task CreateTask_ShouldCreateTask()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var title = Unique("task-create");

        var response = await Api.PostAsync("/api/tasks", new APIRequestContextOptions
        {
            DataObject = new
            {
                calendarId,
                createdByUserId = user.UserId,
                title,
                description = "Created by API test",
                dueUtc = FutureUtc(24).ToString("O"),
                priority = "High",
                status = "Todo",
                completedAtUtc = (string?)null,
                reminderMinutesBefore = 45
            }
        });

        Assert.That(response.Status, Is.EqualTo(201));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonString(json, "title"), Is.EqualTo(title));
    }

    [Test]
    public async Task UpdateTask_ShouldModifyTask()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var taskId = await CreateTaskAsync(calendarId, user.UserId);
        var title = Unique("task-update");

        var response = await Api.PutAsync($"/api/tasks/{taskId}", new APIRequestContextOptions
        {
            DataObject = new
            {
                calendarId,
                createdByUserId = user.UserId,
                title,
                description = "Updated by API test",
                dueUtc = FutureUtc(36).ToString("O"),
                priority = "Low",
                status = "InProgress",
                completedAtUtc = (string?)null,
                reminderMinutesBefore = 10
            }
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonString(json, "title"), Is.EqualTo(title));
        Assert.That(JsonString(json, "status"), Is.EqualTo("InProgress"));
    }

    [Test]
    public async Task UpdateTaskStatus_ShouldMoveTaskToDone()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var taskId = await CreateTaskAsync(calendarId, user.UserId);

        var response = await Api.PutAsync($"/api/tasks/{taskId}/status", new APIRequestContextOptions
        {
            DataObject = new
            {
                status = "Done",
                completedAtUtc = FutureUtc(1).ToString("O")
            }
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonString(json, "status"), Is.EqualTo("Done"));
    }

    [Test]
    public async Task DeleteTask_ShouldRemoveTask()
    {
        var user = await RegisterUserAsync();
        var calendarId = await CreateCalendarAsync(user.UserId);
        var taskId = await CreateTaskAsync(calendarId, user.UserId);

        var deleteResponse = await Api.DeleteAsync($"/api/tasks/{taskId}");
        Assert.That(deleteResponse.Status, Is.EqualTo(204));

        var readResponse = await Api.GetAsync($"/api/tasks/{taskId}");
        Assert.That(readResponse.Status, Is.EqualTo(404));
    }
}
