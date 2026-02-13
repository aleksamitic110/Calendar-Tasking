using CalendarTasking.Api.Data;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
}

builder.Services.AddDbContext<CalendarTaskingDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

if (builder.Configuration.GetValue<bool>("Database:InitializeOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CalendarTaskingDbContext>();
    dbContext.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

var clientPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "client"));
if (Directory.Exists(clientPath))
{
    var clientFileProvider = new PhysicalFileProvider(clientPath);
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = clientFileProvider
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = clientFileProvider
    });
}

app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}
