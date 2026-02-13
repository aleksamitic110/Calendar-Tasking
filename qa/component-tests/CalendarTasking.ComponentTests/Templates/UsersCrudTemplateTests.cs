using System.Net;
using System.Net.Http.Json;
using CalendarTasking.ComponentTests.Infrastructure;

namespace CalendarTasking.ComponentTests.Templates;

public sealed class UsersCrudTemplateTests : ComponentTestBase
{
    private const string TemplateIgnoreReason = "Template only. Implement test logic and remove [Ignore].";
    private const string TemplateFailMessage = "Template test. Add arrange/act/assert and remove [Ignore].";

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
    [Ignore(TemplateIgnoreReason)]
    public void Create_Register_ShouldReturnBadRequest_WhenPayloadIsInvalid_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Create_Register_ShouldReturnConflict_WhenEmailAlreadyExists_Template03()
    {
        Assert.Fail(TemplateFailMessage);
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
    [Ignore(TemplateIgnoreReason)]
    public void ReadAll_ShouldReturnOkAndEmptyList_WhenNoUsersExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadAll_ShouldReturnUsersOrderedById_Template03()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnOk_WhenUserExists_Template01()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnNotFound_WhenUserDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void ReadById_ShouldReturnConsistentUserPayload_Template03()
    {
        Assert.Fail(TemplateFailMessage);
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
    [Ignore(TemplateIgnoreReason)]
    public void Update_ShouldReturnNotFound_WhenUserDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Update_ShouldReturnConflict_WhenEmailIsAlreadyUsed_Template03()
    {
        Assert.Fail(TemplateFailMessage);
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
    [Ignore(TemplateIgnoreReason)]
    public void Delete_ShouldReturnNotFound_WhenUserDoesNotExist_Template02()
    {
        Assert.Fail(TemplateFailMessage);
    }

    [Test]
    [Ignore(TemplateIgnoreReason)]
    public void Delete_ShouldRemoveUser_FromSubsequentReads_Template03()
    {
        Assert.Fail(TemplateFailMessage);
    }
}
