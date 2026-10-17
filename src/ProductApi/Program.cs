using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Products;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ProductDb")
    ?? throw new InvalidOperationException("Connection string 'ProductDb' is not configured.");

builder.Services.AddDbContext<ProductDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddHealthChecks().AddDbContextCheck<ProductDbContext>("database");

var app = builder.Build();

// Creates the schema on first start. Good enough for this assignment; a real product would use EF Core migrations.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// APP_VERSION is set at image build time to the Git commit SHA, so the demo can show which build is running.
var version = app.Configuration["APP_VERSION"] ?? "dev";
app.MapGet("/", () => Results.Ok(new { name = "ProductApi", message = "Hello from the ProductApi!", version }));

app.MapHealthChecks("/health");
app.MapProductEndpoints();

app.Run();

// Makes the Program class visible to WebApplicationFactory in the integration tests.
public partial class Program;
