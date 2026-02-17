namespace CalendarTasking.PlaywrightTests.Infrastructure;

public static class TestConfig
{
    public static string BaseUrl =>
        (Environment.GetEnvironmentVariable("CALENDAR_TASKING_BASE_URL") ?? "http://localhost:5170").TrimEnd('/');
}
