namespace CalendarTasking.ComponentTests.Infrastructure;

public abstract class ComponentTestBase
{
    protected CalendarTaskingApiFactory Factory = null!;
    protected HttpClient Client = null!;

    [SetUp]
    public void BaseSetUp()
    {
        Factory = new CalendarTaskingApiFactory();
        Client = Factory.CreateClient();
    }

    [TearDown]
    public void BaseTearDown()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}
