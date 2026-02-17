using System.Net;
using System.Net.Http.Json;
using CalendarTasking.ComponentTests.Infrastructure;

namespace CalendarTasking.ComponentTests.Templates;

public sealed class UsersCrudTemplateTests : ComponentTestBase
{
    [Test]
    public async Task Create_Register_ShouldReturnCreated_WhenPayloadIsValid_Template01()
    {
        var uniqueEmail = $"users-create-{Guid.NewGuid():N}@example.com";
        var request = new RegisterUserRequestDto(uniqueEmail, "Pass123!", "Ana", "Test", "UTC");

        var response = await Client.PostAsJsonAsync("/api/users/register", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Email, Is.EqualTo(uniqueEmail));
    }

    [Test]
    public async Task Create_Register_ShouldReturnBadRequest_WhenPayloadIsInvalid_Template02()
    {
        var invalidRequest = new RegisterUserRequestDto("not-an-email", "123", "Ana", "Test", "UTC");

        var response = await Client.PostAsJsonAsync("/api/users/register", invalidRequest);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Create_Register_ShouldReturnConflict_WhenEmailAlreadyExists_Template03()
    {
        var email = $"users-duplicate-{Guid.NewGuid():N}@example.com";
        await Client.RegisterUserAsync(email);
        var duplicateRequest = new RegisterUserRequestDto($" {email.ToUpperInvariant()} ", "Pass123!", "Dup", "User", "UTC");

        var response = await Client.PostAsJsonAsync("/api/users/register", duplicateRequest);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task ReadAll_ShouldReturnOkAndUsers_WhenUsersExist_Template01()
    {
        var first = await Client.RegisterUserAsync();
        var second = await Client.RegisterUserAsync();

        var response = await Client.GetAsync("/api/users");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var users = await response.Content.ReadFromJsonAsync<List<UserResponseDto>>();
        Assert.That(users, Is.Not.Null);
        Assert.That(users!, Has.Count.GreaterThanOrEqualTo(2));
        Assert.That(users.Select(x => x.UserId), Does.Contain(first.UserId));
        Assert.That(users.Select(x => x.UserId), Does.Contain(second.UserId));
    }

    [Test]
    public async Task ReadAll_ShouldReturnOkAndEmptyList_WhenNoUsersExist_Template02()
    {
        var response = await Client.GetAsync("/api/users");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var users = await response.Content.ReadFromJsonAsync<List<UserResponseDto>>();
        Assert.That(users, Is.Not.Null);
        Assert.That(users!, Is.Empty);
    }

    [Test]
    public async Task ReadAll_ShouldReturnUsersOrderedById_Template03()
    {
        await Client.RegisterUserAsync();
        await Client.RegisterUserAsync();
        await Client.RegisterUserAsync();

        var response = await Client.GetAsync("/api/users");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var users = await response.Content.ReadFromJsonAsync<List<UserResponseDto>>();
        Assert.That(users, Is.Not.Null);

        var ids = users!.Select(x => x.UserId).ToList();
        Assert.That(ids, Is.Ordered.Ascending);
    }

    [Test]
    public async Task ReadById_ShouldReturnOk_WhenUserExists_Template01()
    {
        var user = await Client.RegisterUserAsync();

        var response = await Client.GetAsync($"/api/users/{user.UserId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var found = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.UserId, Is.EqualTo(user.UserId));
    }

    [Test]
    public async Task ReadById_ShouldReturnNotFound_WhenUserDoesNotExist_Template02()
    {
        var response = await Client.GetAsync("/api/users/999999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ReadById_ShouldReturnConsistentUserPayload_Template03()
    {
        var email = $"users-read-{Guid.NewGuid():N}@example.com";
        var registerRequest = new RegisterUserRequestDto(email, "Pass123!", "Mila", "Nikolic", "Europe/Belgrade");
        var createResponse = await Client.PostAsJsonAsync("/api/users/register", registerRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.That(created, Is.Not.Null);

        var response = await Client.GetAsync($"/api/users/{created!.UserId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetched = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.UserId, Is.EqualTo(created.UserId));
        Assert.That(fetched.Email, Is.EqualTo(email));
        Assert.That(fetched.FirstName, Is.EqualTo("Mila"));
        Assert.That(fetched.LastName, Is.EqualTo("Nikolic"));
        Assert.That(fetched.TimeZoneId, Is.EqualTo("Europe/Belgrade"));
        Assert.That(fetched.IsActive, Is.True);
    }

    [Test]
    public async Task Update_ShouldReturnOk_WhenUserExistsAndPayloadIsValid_Template01()
    {
        var user = await Client.RegisterUserAsync();
        var updateRequest = new UpdateUserRequestDto(
            $"users-update-{Guid.NewGuid():N}@example.com",
            "Updated",
            "User",
            "UTC",
            true);

        var response = await Client.PutAsJsonAsync($"/api/users/{user.UserId}", updateRequest);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Email, Is.EqualTo(updateRequest.Email));
        Assert.That(updated.FirstName, Is.EqualTo("Updated"));
    }

    [Test]
    public async Task Update_ShouldReturnNotFound_WhenUserDoesNotExist_Template02()
    {
        var request = new UpdateUserRequestDto(
            $"missing-user-{Guid.NewGuid():N}@example.com",
            "Missing",
            "User",
            "UTC",
            true);

        var response = await Client.PutAsJsonAsync("/api/users/999999", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_ShouldReturnConflict_WhenEmailIsAlreadyUsed_Template03()
    {
        var first = await Client.RegisterUserAsync();
        var second = await Client.RegisterUserAsync();

        var request = new UpdateUserRequestDto(first.Email, "Second", "User", "UTC", true);

        var response = await Client.PutAsJsonAsync($"/api/users/{second.UserId}", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task Delete_ShouldReturnNoContent_WhenUserExists_Template01()
    {
        var user = await Client.RegisterUserAsync();

        var deleteResponse = await Client.DeleteAsync($"/api/users/{user.UserId}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getResponse = await Client.GetAsync($"/api/users/{user.UserId}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ShouldReturnNotFound_WhenUserDoesNotExist_Template02()
    {
        var response = await Client.DeleteAsync("/api/users/999999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ShouldRemoveUser_FromSubsequentReads_Template03()
    {
        var first = await Client.RegisterUserAsync();
        var second = await Client.RegisterUserAsync();

        var deleteResponse = await Client.DeleteAsync($"/api/users/{first.UserId}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var allResponse = await Client.GetAsync("/api/users");
        Assert.That(allResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var users = await allResponse.Content.ReadFromJsonAsync<List<UserResponseDto>>();
        Assert.That(users, Is.Not.Null);
        Assert.That(users!.Select(x => x.UserId), Does.Not.Contain(first.UserId));
        Assert.That(users.Select(x => x.UserId), Does.Contain(second.UserId));
    }
}
