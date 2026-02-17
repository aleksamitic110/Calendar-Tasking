using CalendarTasking.PlaywrightTests.Infrastructure;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace CalendarTasking.PlaywrightTests.Ui;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class UiFlowsTests : PageTest
{
    private static string LocalInput(DateTime value) => value.ToString("yyyy-MM-ddTHH:mm");

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private async Task GoToAppAsync()
    {
        await Page.GotoAsync($"{TestConfig.BaseUrl}/");
        await Page.EvaluateAsync("() => localStorage.removeItem('calendar_tasking_user_v1')");
        await Page.ReloadAsync();
        Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
    }

    private async Task LoginWithSeedUserAsync()
    {
        await GoToAppAsync();
        await Page.GetByLabel("Email").FillAsync("ana@example.com");
        await Page.GetByLabel("Password").FillAsync("Pass123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Jack In" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "System Overview" })).ToBeVisibleAsync();
    }

    private async Task OpenSectionAsync(string sectionName)
    {
        var menuButton = Page.Locator("aside .menu .menu-btn", new() { HasText = sectionName }).First;
        if (!await menuButton.IsVisibleAsync())
        {
            await Page.GetByRole(AriaRole.Button, new() { Name = "Toggle menu" }).ClickAsync();
        }

        await Expect(menuButton).ToBeVisibleAsync();
        await menuButton.ClickAsync();
    }

    private async Task<ILocator> OpenSectionAndGetVisibleFormAsync(string sectionName, string formHeading)
    {
        await OpenSectionAsync(sectionName);

        var section = Page.Locator("section.page:visible").First;
        await Expect(section).ToBeVisibleAsync();

        var heading = section.GetByRole(AriaRole.Heading, new() { Name = formHeading, Exact = true });
        await Expect(heading).ToBeVisibleAsync();

        return section;
    }

    private static ILocator GetFormPanel(ILocator section, string formHeading)
    {
        var saveButtonText = formHeading switch
        {
            "New calendar" => "Save calendar",
            "New task" => "Save task",
            "New event" => "Save event",
            "New private class session" => "Save session",
            _ => "Save"
        };

        return section.Locator($"form:has(button:has-text('{saveButtonText}'))").First;
    }

    [Test]
    public async Task Login_And_Logout_ShouldWork()
    {
        await LoginWithSeedUserAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Logout" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Jack In" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_ShouldAutoLogin()
    {
        var email = $"{Unique("ui-register")}@example.com";

        await GoToAppAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password").FillAsync("Pass123!");
        await Page.GetByLabel("First name").FillAsync("Playwright");
        await Page.GetByLabel("Last name").FillAsync("Tester");
        await Page.GetByLabel("Time zone").FillAsync("UTC");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Identity" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "System Overview" })).ToBeVisibleAsync();
        await Expect(Page.Locator(".user-card")).ToContainTextAsync(email);
    }

    [Test]
    public async Task Calendars_Crud_ShouldWork()
    {
        var name = Unique("ui-cal");
        var updatedName = $"{name}-updated";

        await LoginWithSeedUserAsync();
        var section = await OpenSectionAndGetVisibleFormAsync("Calendars", "New calendar");

        var form = GetFormPanel(section, "New calendar");
        await form.Locator("label:has-text('Name') input").FillAsync(name);
        await form.Locator("label:has-text('Description') textarea").FillAsync("Created by UI test");
        await form.Locator("label:has-text('Color') input[type='color']").FillAsync("#2255aa");
        await form.GetByRole(AriaRole.Button, new() { Name = "Save calendar" }).ClickAsync();

        var createdCard = section.Locator(".item-card").Filter(new() { HasText = name }).First;
        await Expect(createdCard).ToBeVisibleAsync();

        await createdCard.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        await form.Locator("label:has-text('Name') input").FillAsync(updatedName);
        await form.GetByRole(AriaRole.Button, new() { Name = "Save calendar" }).ClickAsync();

        var updatedCard = section.Locator(".item-card").Filter(new() { HasText = updatedName }).First;
        await Expect(updatedCard).ToBeVisibleAsync();

        await updatedCard.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        await Expect(updatedCard).ToHaveCountAsync(0);
    }

    [Test]
    public async Task Tasks_Crud_And_Status_ShouldWork()
    {
        var title = Unique("ui-task");
        var updatedTitle = $"{title}-updated";

        await LoginWithSeedUserAsync();
        var section = await OpenSectionAndGetVisibleFormAsync("Tasks", "New task");

        var form = GetFormPanel(section, "New task");
        await form.Locator("label:has-text('Title') input").FillAsync(title);
        await form.Locator("label:has-text('Description') textarea").FillAsync("Task from UI test");
        await form.Locator("label:has-text('Due') input").FillAsync(LocalInput(DateTime.Now.AddDays(1)));
        await form.Locator("label:has-text('Priority') select").SelectOptionAsync("High");
        await form.Locator("label:has-text('Status') select").SelectOptionAsync("Todo");
        await form.GetByRole(AriaRole.Button, new() { Name = "Save task" }).ClickAsync();

        var createdCard = section.Locator(".item-card").Filter(new() { HasText = title }).First;
        await Expect(createdCard).ToBeVisibleAsync();

        await createdCard.GetByRole(AriaRole.Button, new() { Name = "Done" }).ClickAsync();
        await Expect(createdCard).ToContainTextAsync("Done");

        await createdCard.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        await form.Locator("label:has-text('Title') input").FillAsync(updatedTitle);
        await form.GetByRole(AriaRole.Button, new() { Name = "Save task" }).ClickAsync();

        var updatedCard = section.Locator(".item-card").Filter(new() { HasText = updatedTitle }).First;
        await Expect(updatedCard).ToBeVisibleAsync();

        await updatedCard.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        await Expect(updatedCard).ToHaveCountAsync(0);
    }

    [Test]
    public async Task Events_Crud_ShouldWork()
    {
        var title = Unique("ui-event");
        var updatedTitle = $"{title}-updated";

        await LoginWithSeedUserAsync();
        var section = await OpenSectionAndGetVisibleFormAsync("Events", "New event");

        var form = GetFormPanel(section, "New event");
        await form.Locator("label:has-text('Title') input").FillAsync(title);
        await form.Locator("label:has-text('Description') textarea").FillAsync("Event from UI test");
        await form.Locator("label:has-text('Location') input").FillAsync("Classroom A");
        await form.Locator("label:has-text('Start') input").FillAsync(LocalInput(DateTime.Now.AddHours(2)));
        await form.Locator("label:has-text('End') input").FillAsync(LocalInput(DateTime.Now.AddHours(3)));
        await form.GetByRole(AriaRole.Button, new() { Name = "Save event" }).ClickAsync();

        var createdCard = section.Locator(".item-card").Filter(new() { HasText = title }).First;
        await Expect(createdCard).ToBeVisibleAsync();

        await createdCard.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        await form.Locator("label:has-text('Title') input").FillAsync(updatedTitle);
        await form.GetByRole(AriaRole.Button, new() { Name = "Save event" }).ClickAsync();

        var updatedCard = section.Locator(".item-card").Filter(new() { HasText = updatedTitle }).First;
        await Expect(updatedCard).ToBeVisibleAsync();

        await updatedCard.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        await Expect(updatedCard).ToHaveCountAsync(0);
    }

    [Test]
    public async Task Sessions_Crud_And_PaymentActions_ShouldWork()
    {
        var student = Unique("ui-student");
        var updatedStudent = $"{student}-updated";

        await LoginWithSeedUserAsync();
        var section = await OpenSectionAndGetVisibleFormAsync("Sessions", "New private class session");

        var form = GetFormPanel(section, "New private class session");
        await form.Locator("label:has-text('Student name') input").FillAsync(student);
        await form.Locator("label:has-text('Student contact') input").FillAsync("student@example.com");
        await form.Locator("label:has-text('Session start') input").FillAsync(LocalInput(DateTime.Now.AddHours(4)));
        await form.Locator("label:has-text('Session end') input").FillAsync(LocalInput(DateTime.Now.AddHours(5)));
        await form.Locator("label:has-text('Price') input").FillAsync("3000");
        await form.Locator("label:has-text('Currency') input").FillAsync("RSD");
        await form.GetByRole(AriaRole.Button, new() { Name = "Save session" }).ClickAsync();

        var createdCard = section.Locator(".item-card").Filter(new() { HasText = student }).First;
        await Expect(createdCard).ToBeVisibleAsync();

        await createdCard.GetByRole(AriaRole.Button, new() { Name = "Mark paid" }).ClickAsync();
        await Expect(createdCard).ToContainTextAsync("Paid");

        await createdCard.GetByRole(AriaRole.Button, new() { Name = "Mark unpaid" }).ClickAsync();
        await Expect(createdCard).ToContainTextAsync("Unpaid");

        await createdCard.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        await form.Locator("label:has-text('Student name') input").FillAsync(updatedStudent);
        await form.GetByRole(AriaRole.Button, new() { Name = "Save session" }).ClickAsync();

        var updatedCard = section.Locator(".item-card").Filter(new() { HasText = updatedStudent }).First;
        await Expect(updatedCard).ToBeVisibleAsync();

        await updatedCard.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        await Expect(updatedCard).ToHaveCountAsync(0);
    }
}
