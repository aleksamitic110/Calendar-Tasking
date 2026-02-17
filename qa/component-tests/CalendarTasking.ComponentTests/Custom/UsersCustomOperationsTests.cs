using System.Net;
using System.Net.Http.Json;
using CalendarTasking.ComponentTests.Infrastructure;

namespace CalendarTasking.ComponentTests.Custom;

public sealed class UsersCustomOperationsTests : ComponentTestBase
{
    [Test]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
    {
        var email = $"users-login-{Guid.NewGuid():N}@example.com";
        var user = await Client.RegisterUserAsync(email);
        var request = new LoginUserRequestTestDto(email, "Pass123!");

        var response = await Client.PostAsJsonAsync("/api/users/login", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var loggedIn = await response.Content.ReadFromJsonAsync<LoginUserResponseTestDto>();
        Assert.That(loggedIn, Is.Not.Null);
        Assert.That(loggedIn!.UserId, Is.EqualTo(user.UserId));
        Assert.That(loggedIn.Email, Is.EqualTo(email));
        Assert.That(loggedIn.FullName, Is.EqualTo("Test User"));
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        var email = $"users-login-wrong-{Guid.NewGuid():N}@example.com";
        await Client.RegisterUserAsync(email);

        var response = await Client.PostAsJsonAsync("/api/users/login", new LoginUserRequestTestDto(email, "WrongPass123!"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenUserIsInactive()
    {
        var email = $"users-login-inactive-{Guid.NewGuid():N}@example.com";
        var user = await Client.RegisterUserAsync(email);

        var deactivateRequest = new UpdateUserRequestDto(email, "Test", "User", "UTC", false);
        var deactivateResponse = await Client.PutAsJsonAsync($"/api/users/{user.UserId}", deactivateRequest);
        Assert.That(deactivateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var response = await Client.PostAsJsonAsync("/api/users/login", new LoginUserRequestTestDto(email, "Pass123!"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ChangePassword_ShouldReturnNoContent_WhenCurrentPasswordIsValid()
    {
        var email = $"users-password-{Guid.NewGuid():N}@example.com";
        var user = await Client.RegisterUserAsync(email);

        var changeResponse = await Client.PutAsJsonAsync(
            $"/api/users/{user.UserId}/password",
            new ChangePasswordRequestTestDto("Pass123!", "NewPass123!"));

        Assert.That(changeResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var oldLogin = await Client.PostAsJsonAsync("/api/users/login", new LoginUserRequestTestDto(email, "Pass123!"));
        var newLogin = await Client.PostAsJsonAsync("/api/users/login", new LoginUserRequestTestDto(email, "NewPass123!"));

        Assert.That(oldLogin.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(newLogin.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ChangePassword_ShouldReturnBadRequest_WhenCurrentPasswordIsWrong()
    {
        var user = await Client.RegisterUserAsync();

        var response = await Client.PutAsJsonAsync(
            $"/api/users/{user.UserId}/password",
            new ChangePasswordRequestTestDto("WrongPass123!", "NewPass123!"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ChangePassword_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var response = await Client.PutAsJsonAsync(
            "/api/users/999999/password",
            new ChangePasswordRequestTestDto("Pass123!", "NewPass123!"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}

public sealed record LoginUserRequestTestDto(string Email, string Password);
public sealed record LoginUserResponseTestDto(int UserId, string Email, string FullName);
public sealed record ChangePasswordRequestTestDto(string CurrentPassword, string NewPassword);
