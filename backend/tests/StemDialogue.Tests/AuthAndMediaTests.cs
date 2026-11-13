using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace StemDialogue.Tests;

public sealed class AuthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Wrong_password_is_rejected()
    {
        var response = await factory.Anonymous().PostAsJsonAsync("/api/v1/auth/login", new { email = ApiFactory.AdminEmail, password = "wrong-password-1" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_sets_a_secure_refresh_cookie_that_rotates_and_detects_reuse()
    {
        var client = factory.Anonymous();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { email = ApiFactory.AdminEmail, password = ApiFactory.AdminPassword });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var cookie = login.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("sd_refresh=", StringComparison.Ordinal));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        var original = cookie.Split(';')[0];

        // The client's cookie container now holds the cookie; refresh rotates it.
        var refreshed = await client.PostAsync("/api/v1/auth/refresh", null);
        Assert.Equal(HttpStatusCode.OK, refreshed.StatusCode);

        // A rotated (old) token is no longer accepted.
        var replay = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        replay.Headers.Add("Cookie", original);
        var fresh = factory.Anonymous();
        await Task.Delay(TimeSpan.FromMilliseconds(50));
        var replayed = await fresh.SendAsync(replay);
        Assert.Equal(HttpStatusCode.Unauthorized, replayed.StatusCode);
    }

    [Fact]
    public async Task Logout_invalidates_the_access_token()
    {
        var admin = await factory.AdminAsync();
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/v1/auth/me")).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await admin.PostAsync("/api/v1/auth/logout", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await admin.GetAsync("/api/v1/auth/me")).StatusCode);
    }
}

public sealed class MediaTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly byte[] OnePixelPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    [Fact]
    public async Task Png_upload_is_accepted_with_dimensions_and_served()
    {
        var admin = await factory.AdminAsync();
        var media = await (await admin.PostAsync("/api/v1/admin/media", Form("logo.png", OnePixelPng, "image/png", "ლოგო"))).JsonAsync();

        Assert.Equal(1, media.GetProperty("width").GetInt32());
        Assert.Equal("image/png", media.Str("contentType"));

        var file = await factory.Anonymous().GetAsync(media.Str("url"));
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
        Assert.Equal("nosniff", file.Headers.GetValues("X-Content-Type-Options").Single());
    }

    [Fact]
    public async Task File_whose_content_does_not_match_its_extension_is_rejected()
    {
        var admin = await factory.AdminAsync();
        var response = await admin.PostAsync("/api/v1/admin/media", Form("photo.png", Encoding.UTF8.GetBytes("not an image"), "image/png"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Svg_with_script_is_rejected()
    {
        var admin = await factory.AdminAsync();
        var svg = Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>");
        var response = await admin.PostAsync("/api/v1/admin/media", Form("logo.svg", svg, "image/svg+xml"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Executable_extension_is_rejected()
    {
        var admin = await factory.AdminAsync();
        var response = await admin.PostAsync("/api/v1/admin/media", Form("tool.exe", [0x4D, 0x5A, 0x90, 0x00], "application/octet-stream"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Used_media_cannot_be_deleted()
    {
        var admin = await factory.AdminAsync();
        var media = await (await admin.PostAsync("/api/v1/admin/media", Form("cover.png", OnePixelPng, "image/png", "ყდა"))).JsonAsync();
        await (await admin.PostJsonAsync("/api/v1/admin/projects/school2university/news", new
        {
            title = new { ka = "სიახლე სურათით" },
            category = "General",
            coverImageId = media.Str("id"),
        })).JsonAsync();

        var delete = await admin.DeleteAsync($"/api/v1/admin/media/{media.Str("id")}");
        Assert.Equal(HttpStatusCode.Conflict, delete.StatusCode);
    }

    private static MultipartFormDataContent Form(string name, byte[] bytes, string contentType, string? alt = null)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        var form = new MultipartFormDataContent { { file, "file", name } };
        if (alt is not null) form.Add(new StringContent(alt), "altKa");
        return form;
    }
}
