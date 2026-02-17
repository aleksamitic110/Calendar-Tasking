using System.Net;
using System.Net.Http.Json;
using CalendarTasking.ComponentTests.Infrastructure;

namespace CalendarTasking.ComponentTests.Custom;

public sealed class TasksCustomOperationsTests : ComponentTestBase
{
    [Test]
    public async Task UpdateStatus_ShouldReturnOkAndSetCompletedAt_WhenStatusIsDone()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var task = await Client.CreateTaskAsync(calendar.CalendarId, user.UserId);

        var response = await Client.PutAsJsonAsync(
            $"/api/tasks/{task.TaskItemId}/status",
            new UpdateTaskStatusRequestTestDto("done", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Status, Is.EqualTo("Done"));
        Assert.That(updated.CompletedAtUtc, Is.Not.Null);
    }

    [Test]
    public async Task UpdateStatus_ShouldReturnBadRequest_WhenStatusIsInvalid()
    {
        var user = await Client.RegisterUserAsync();
        var calendar = await Client.CreateCalendarAsync(user.UserId);
        var task = await Client.CreateTaskAsync(calendar.CalendarId, user.UserId);

        var response = await Client.PutAsJsonAsync(
            $"/api/tasks/{task.TaskItemId}/status",
            new UpdateTaskStatusRequestTestDto("InvalidStatus", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task UpdateStatus_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        var response = await Client.PutAsJsonAsync(
            "/api/tasks/999999/status",
            new UpdateTaskStatusRequestTestDto("Done", DateTime.UtcNow));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}

public sealed record UpdateTaskStatusRequestTestDto(string Status, DateTime? CompletedAtUtc);
