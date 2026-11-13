using StemDialogue.Api.Web;
using StemDialogue.Application;
using StemDialogue.Infrastructure;
using StemDialogue.Infrastructure.Persistence;
using StemDialogue.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWeb(builder.Configuration, builder.Environment);

var app = builder.Build();

// One-off maintenance commands, run as a separate deployment job:
//   --migrate  apply migrations, roles, bootstrap admin and base project content
//   --seed     the same plus clearly marked demo content (Development only)
if (args.Contains("--migrate") || args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    var demo = args.Contains("--seed") && app.Environment.IsDevelopment();
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync(demo);
    return;
}

// SQLite (local development) creates its schema on start; SQL Server is migrated by the job above.
if (string.Equals(app.Configuration["Database:Provider"], "Sqlite", StringComparison.OrdinalIgnoreCase)
    && app.Configuration.GetValue("Database:InitializeOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync(app.Environment.IsDevelopment());
}

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSecurityHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.DocumentTitle = "STEM Dialogue API");
}

app.UseMediaFiles();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

app.MapControllers();
app.MapGet("/health", async (AppDbContext db, CancellationToken ct) =>
        await db.Database.CanConnectAsync(ct) ? Results.Ok(new { status = "ok" }) : Results.Problem("Database unavailable", statusCode: 503))
    .AllowAnonymous()
    .ExcludeFromDescription();

app.Run();

/// <summary>Entry point marker for integration tests.</summary>
public partial class Program;
