using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace StemDialogue.Tests;

/// <summary>
/// Runs the real API in memory against a private shared-cache SQLite database, seeded exactly like
/// production (roles, bootstrap admin, SPG-26-185 project content).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "admin@tests.local";
    public const string AdminPassword = "Test-password-123";
    public const string Project = "school2university";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private readonly string _database = $"file:stemdialogue-{Guid.NewGuid():N}?mode=memory&cache=shared";
    private readonly string _media = Path.Combine(Path.GetTempPath(), "stemdialogue-tests", Guid.NewGuid().ToString("N"));
    private SqliteConnection? _keepAlive;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Database:Provider", "Sqlite");
        builder.UseSetting("Database:InitializeOnStartup", "true");
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_database}");
        builder.UseSetting("Jwt:Key", "tests-only-signing-key-0123456789-abcdefghijklmnop");
        builder.UseSetting("Bootstrap:Email", AdminEmail);
        builder.UseSetting("Bootstrap:Password", AdminPassword);
        builder.UseSetting("Storage:RootPath", _media);
        builder.UseSetting("RateLimiting:AuthPerMinute", "10000");
        builder.UseSetting("RateLimiting:RequestsPerMinute", "100000");
    }

    public Task InitializeAsync()
    {
        // Keeps the in-memory database alive for the lifetime of the factory.
        _keepAlive = new SqliteConnection($"Data Source={_database}");
        _keepAlive.Open();
        return Task.CompletedTask;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        _keepAlive?.Dispose();
        if (Directory.Exists(_media)) Directory.Delete(_media, recursive: true);
    }

    /// <summary>HTTPS client (the refresh cookie is Secure) that does not follow redirects.</summary>
    public HttpClient Anonymous() =>
        CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });

    public async Task<HttpClient> AdminAsync()
    {
        var client = Anonymous();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email = AdminEmail, password = AdminPassword });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.GetProperty("accessToken").GetString());
        return client;
    }
}

internal static class HttpExtensions
{
    public static async Task<JsonElement> JsonAsync(this HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.ReasonPhrase}: {text}");
        return JsonDocument.Parse(text).RootElement.Clone();
    }

    public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string url, object body) =>
        client.PostAsJsonAsync(url, body, ApiFactory.Json);

    public static Task<HttpResponseMessage> PutJsonAsync(this HttpClient client, string url, object body) =>
        client.PutAsJsonAsync(url, body, ApiFactory.Json);

    public static string Str(this JsonElement element, string property) => element.GetProperty(property).GetString()!;
}
