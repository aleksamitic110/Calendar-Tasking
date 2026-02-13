using CalendarTasking.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CalendarTasking.ComponentTests.Infrastructure;

public sealed class CalendarTaskingApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=CalendarTaskingDbTests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["Database:InitializeOnStartup"] = "false"
            };

            configBuilder.AddInMemoryCollection(inMemorySettings);
        });

        builder.ConfigureServices(services =>
        {
            var databaseName = $"calendar-tasking-tests-{Guid.NewGuid():N}";

            services.RemoveAll(typeof(IDbContextOptionsConfiguration<CalendarTaskingDbContext>));
            services.RemoveAll(typeof(DbContextOptions<CalendarTaskingDbContext>));
            services.AddDbContext<CalendarTaskingDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
        });
    }
}
