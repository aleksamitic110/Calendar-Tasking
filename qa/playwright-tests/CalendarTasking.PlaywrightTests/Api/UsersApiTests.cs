using CalendarTasking.PlaywrightTests.Infrastructure;
using Microsoft.Playwright;

namespace CalendarTasking.PlaywrightTests.Api;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class UsersApiTests : PlaywrightApiTestBase
{
    [Test]
    public async Task GetUsers_ShouldReturnCreatedUser()
    {
        var created = await RegisterUserAsync();

        var response = await Api.GetAsync("/api/users");

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        var users = json!.AsArray();
        Assert.That(users.Any(x => x?["userId"]?.GetValue<int>() == created.UserId), Is.True);
    }

    [Test]
    public async Task GetUserById_ShouldReturnUser()
    {
        var created = await RegisterUserAsync();

        var response = await Api.GetAsync($"/api/users/{created.UserId}");

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonInt(json, "userId"), Is.EqualTo(created.UserId));
    }

    [Test]
    public async Task Register_ShouldCreateUser()
    {
        var email = $"{Unique("users-register")}@example.com";
        var response = await Api.PostAsync("/api/users/register", new APIRequestContextOptions
        {
            DataObject = new
            {
                email,
                password = "Pass123!",
                firstName = "Register",
                lastName = "Case",
                timeZoneId = "UTC"
            }
        });

        Assert.That(response.Status, Is.EqualTo(201));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonString(json, "email"), Is.EqualTo(email));
    }

    [Test]
    public async Task Login_ShouldReturnOkForValidCredentials()
    {
        var created = await RegisterUserAsync();
        var response = await Api.PostAsync("/api/users/login", new APIRequestContextOptions
        {
            DataObject = new
            {
                email = created.Email,
                password = "Pass123!"
            }
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonInt(json, "userId"), Is.EqualTo(created.UserId));
    }

    [Test]
    public async Task Update_ShouldModifyUser()
    {
        var created = await RegisterUserAsync();
        var newEmail = $"{Unique("users-update")}@example.com";

        var response = await Api.PutAsync($"/api/users/{created.UserId}", new APIRequestContextOptions
        {
            DataObject = new
            {
                email = newEmail,
                firstName = "Updated",
                lastName = "User",
                timeZoneId = "Europe/Belgrade",
                isActive = true
            }
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await ReadJsonAsync(response);
        Assert.That(JsonString(json, "email"), Is.EqualTo(newEmail));
        Assert.That(JsonString(json, "firstName"), Is.EqualTo("Updated"));
    }

    [Test]
    public async Task ChangePassword_ShouldRequireNewPasswordForLogin()
    {
        var created = await RegisterUserAsync();

        var changeResponse = await Api.PutAsync($"/api/users/{created.UserId}/password", new APIRequestContextOptions
        {
            DataObject = new
            {
                currentPassword = "Pass123!",
                newPassword = "NewPass123!"
            }
        });

        Assert.That(changeResponse.Status, Is.EqualTo(204));

        var oldLogin = await Api.PostAsync("/api/users/login", new APIRequestContextOptions
        {
            DataObject = new { email = created.Email, password = "Pass123!" }
        });
        var newLogin = await Api.PostAsync("/api/users/login", new APIRequestContextOptions
        {
            DataObject = new { email = created.Email, password = "NewPass123!" }
        });

        Assert.That(oldLogin.Status, Is.EqualTo(401));
        Assert.That(newLogin.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task Delete_ShouldRemoveUser()
    {
        var created = await RegisterUserAsync();

        var deleteResponse = await Api.DeleteAsync($"/api/users/{created.UserId}");
        Assert.That(deleteResponse.Status, Is.EqualTo(204));

        var readResponse = await Api.GetAsync($"/api/users/{created.UserId}");
        Assert.That(readResponse.Status, Is.EqualTo(404));
    }
}
