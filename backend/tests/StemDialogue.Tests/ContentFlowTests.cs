using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace StemDialogue.Tests;

/// <summary>Acceptance criteria from the specification (§15), exercised through HTTP.</summary>
public sealed class ContentFlowTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Admin = "/api/v1/admin/projects/" + ApiFactory.Project;
    private const string Public = "/api/v1/projects/" + ApiFactory.Project;

    [Fact]
    public async Task Site_shows_funder_and_partner_universities_in_separate_roles()
    {
        var site = await (await factory.Anonymous().GetAsync(Public)).JsonAsync();
        var funding = site.GetProperty("funding");

        Assert.Equal("SPG-26-185", site.Str("code"));
        Assert.Single(funding.GetProperty("funders").EnumerateArray());
        Assert.Equal(2, funding.GetProperty("partners").GetArrayLength());
        Assert.Contains("რუსთაველის", funding.Str("shortText"));
        Assert.Contains("აწსუ", funding.Str("fullText"));
        Assert.Equal(10, site.GetProperty("navigation").GetArrayLength());
    }

    [Fact]
    public async Task English_falls_back_to_georgian_and_uses_english_when_present()
    {
        var site = await (await factory.Anonymous().GetAsync(Public + "?lang=en")).JsonAsync();
        Assert.Equal("Scientific Dialogue — From School to University", site.Str("name"));
        Assert.Equal("en", site.Str("language"));
    }

    [Fact]
    public async Task Admin_creates_and_publishes_news_without_code_changes()
    {
        var admin = await factory.AdminAsync();
        var anonymous = factory.Anonymous();

        var created = await (await admin.PostJsonAsync(Admin + "/news", new
        {
            title = new { ka = "STEM შაბათობა: ფიზიკა" },
            summary = new { ka = "მოკლე ანოტაცია" },
            body = new { ka = "<p>ტექსტი</p><script>alert(1)</script>" },
            category = "Announcement",
        })).JsonAsync();

        var id = created.Str("id");
        Assert.Equal("stem-shabatoba-pizika", created.Str("slug"));
        Assert.DoesNotContain("<script", created.GetProperty("body").Str("ka"));

        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync(Public + "/news/stem-shabatoba-pizika")).StatusCode);

        await (await admin.PostJsonAsync($"{Admin}/news/{id}/publish", new { })).JsonAsync();

        var detail = await (await anonymous.GetAsync(Public + "/news/stem-shabatoba-pizika")).JsonAsync();
        Assert.Equal("STEM შაბათობა: ფიზიკა", detail.Str("title"));
        Assert.Equal("/projects/school2university/news/stem-shabatoba-pizika", detail.GetProperty("seo").Str("canonicalPath"));

        var list = await (await anonymous.GetAsync(Public + "/news?category=Announcement")).JsonAsync();
        Assert.Contains(list.GetProperty("items").EnumerateArray(), n => n.Str("slug") == "stem-shabatoba-pizika");
    }

    [Fact]
    public async Task Scheduled_news_stays_hidden_until_its_publish_time()
    {
        var admin = await factory.AdminAsync();
        var created = await (await admin.PostJsonAsync(Admin + "/news", new
        {
            title = new { ka = "მომავალი განცხადება" },
            summary = new { ka = "ანოტაცია" },
            body = new { ka = "<p>ტექსტი</p>" },
            category = "General",
        })).JsonAsync();

        await (await admin.PostJsonAsync($"{Admin}/news/{created.Str("id")}/publish", new { publishAtUtc = DateTime.UtcNow.AddDays(3) })).JsonAsync();

        var response = await factory.Anonymous().GetAsync($"{Public}/news/{created.Str("slug")}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Renamed_slug_of_published_news_redirects_from_the_old_url()
    {
        var admin = await factory.AdminAsync();
        var created = await (await admin.PostJsonAsync(Admin + "/news", new
        {
            title = new { ka = "ძველი სათაური" },
            summary = new { ka = "ანოტაცია" },
            body = new { ka = "<p>ტექსტი</p>" },
            category = "General",
        })).JsonAsync();
        var id = created.Str("id");
        var published = await (await admin.PostJsonAsync($"{Admin}/news/{id}/publish", new { })).JsonAsync();

        var stamp = published.GetProperty("audit").Str("concurrencyStamp");
        await (await admin.PutJsonAsync($"{Admin}/news/{id}", new
        {
            title = new { ka = "ახალი სათაური" },
            slug = "new-title",
            summary = new { ka = "ანოტაცია" },
            body = new { ka = "<p>ტექსტი</p>" },
            category = "General",
            concurrencyStamp = stamp,
        })).JsonAsync();

        var old = await factory.Anonymous().GetAsync($"{Public}/news/{created.Str("slug")}");
        Assert.Equal(HttpStatusCode.MovedPermanently, old.StatusCode);
        Assert.EndsWith("/news/new-title", old.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Stale_concurrency_stamp_is_rejected()
    {
        var admin = await factory.AdminAsync();
        var created = await (await admin.PostJsonAsync(Admin + "/news", new { title = new { ka = "კონკურენცია" }, category = "General" })).JsonAsync();

        var response = await admin.PutJsonAsync($"{Admin}/news/{created.Str("id")}", new
        {
            title = new { ka = "ცვლილება" },
            category = "General",
            concurrencyStamp = Guid.NewGuid(),
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Validation_errors_are_reported_per_field()
    {
        var admin = await factory.AdminAsync();
        var response = await admin.PostJsonAsync(Admin + "/news", new { title = new { ka = "" }, slug = "Not Valid", category = "General" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(problem.GetProperty("errors").TryGetProperty("title", out _));
        Assert.True(problem.GetProperty("errors").TryGetProperty("slug", out _));
    }

    [Fact]
    public async Task Event_is_announced_then_archived_with_materials_at_the_same_url()
    {
        var admin = await factory.AdminAsync();
        var anonymous = factory.Anonymous();

        // A future event cannot be completed.
        var future = await (await admin.PostJsonAsync(Admin + "/events", new
        {
            type = "StemSaturday",
            title = new { ka = "მომავალი შეხვედრა" },
            summary = new { ka = "ანონსი" },
            startsAtUtc = DateTime.UtcNow.AddDays(10),
            format = "Online",
            registrationUrl = "https://forms.gle/example",
        })).JsonAsync();
        await (await admin.PostJsonAsync($"{Admin}/events/{future.Str("id")}/publish", new { })).JsonAsync();
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PostJsonAsync($"{Admin}/events/{future.Str("id")}/complete", new { })).StatusCode);

        var upcoming = await (await anonymous.GetAsync(Public + "/stem-saturdays?scope=Upcoming")).JsonAsync();
        var card = upcoming.GetProperty("items").EnumerateArray().Single(e => e.Str("slug") == future.Str("slug"));
        Assert.Equal("https://forms.gle/example", card.Str("registrationUrl"));
        Assert.Equal(4, card.GetProperty("startsAt").GetDateTimeOffset().Offset.Hours); // Tbilisi time

        // A held event is completed with a recap and recording; it moves to the archive at the same slug.
        var held = await (await admin.PostJsonAsync(Admin + "/events", new
        {
            type = "StemSaturday",
            title = new { ka = "ჩატარებული შეხვედრა" },
            summary = new { ka = "ანონსი" },
            startsAtUtc = DateTime.UtcNow.AddDays(-3),
            format = "Online",
        })).JsonAsync();
        var heldId = held.Str("id");
        await (await admin.PostJsonAsync($"{Admin}/events/{heldId}/publish", new { })).JsonAsync();
        await (await admin.PostJsonAsync($"{Admin}/events/{heldId}/complete", new
        {
            recap = new { ka = "<p>შეჯამება</p>" },
            recordingUrl = "https://www.youtube.com/watch?v=abcdefghijk",
        })).JsonAsync();

        var past = await (await anonymous.GetAsync(Public + "/stem-saturdays?scope=Past")).JsonAsync();
        Assert.Contains(past.GetProperty("items").EnumerateArray(), e => e.Str("slug") == held.Str("slug"));

        var detail = await (await anonymous.GetAsync($"{Public}/stem-saturdays/{held.Str("slug")}")).JsonAsync();
        Assert.Equal("Completed", detail.Str("status"));
        Assert.Equal("<p>შეჯამება</p>", detail.Str("recapHtml"));
        Assert.Equal("https://www.youtube-nocookie.com/embed/abcdefghijk", detail.Str("recordingEmbedUrl"));
    }

    [Fact]
    public async Task One_person_can_be_researcher_and_mentor_and_profile_lists_their_events()
    {
        var admin = await factory.AdminAsync();
        var anonymous = factory.Anonymous();
        var projectId = (await (await admin.GetAsync(Admin)).JsonAsync()).Str("id");

        var person = await (await admin.PostJsonAsync("/api/v1/admin/people", new
        {
            fullName = new { ka = "ანა მეცნიერიძე", en = "Ana Metsnieridze" },
            status = "Published",
            roles = new object[]
            {
                new { projectId, role = "Researcher", displayOrder = 0 },
                new { projectId, role = "Mentor", displayOrder = 0 },
            },
        })).JsonAsync();
        var slug = person.Str("slug");
        Assert.Equal("ana-metsnieridze", slug);

        var eventResponse = await (await admin.PostJsonAsync(Admin + "/events", new
        {
            type = "StemSaturday",
            title = new { ka = "შეხვედრა ანასთან" },
            summary = new { ka = "ანონსი" },
            startsAtUtc = DateTime.UtcNow.AddDays(5),
            format = "Online",
            participants = new[] { new { personId = person.Str("id"), role = "Speaker" } },
        })).JsonAsync();
        await (await admin.PostJsonAsync($"{Admin}/events/{eventResponse.Str("id")}/publish", new { })).JsonAsync();

        var researchers = await (await anonymous.GetAsync(Public + "/researchers")).JsonAsync();
        var mentors = await (await anonymous.GetAsync(Public + "/mentors")).JsonAsync();
        Assert.Contains(researchers.GetProperty("items").EnumerateArray(), p => p.Str("slug") == slug);
        Assert.Contains(mentors.GetProperty("items").EnumerateArray(), p => p.Str("slug") == slug);

        var profile = await (await anonymous.GetAsync($"{Public}/people/{slug}")).JsonAsync();
        Assert.Equal(2, profile.GetProperty("roles").GetArrayLength());
        Assert.Single(profile.GetProperty("upcomingEvents").EnumerateArray());
        Assert.False(profile.TryGetProperty("email", out _));
    }

    [Fact]
    public async Task Team_page_separates_official_core_team_from_other_sections()
    {
        var team = await (await factory.Anonymous().GetAsync(Public + "/team")).JsonAsync();
        var core = team.GetProperty("sections").EnumerateArray().Single(s => s.Str("role") == "CoreTeam");

        Assert.True(core.GetProperty("isOfficialGrantStaff").GetBoolean());
        Assert.Equal(4, core.GetProperty("current").GetArrayLength());
        Assert.Equal("პროექტის ხელმძღვანელი", core.GetProperty("current")[0].Str("roleTitle"));
    }

    [Fact]
    public async Task Home_hides_nothing_it_does_not_have()
    {
        var home = await (await factory.Anonymous().GetAsync(Public + "/home")).JsonAsync();
        Assert.Equal(JsonValueKind.Object, home.GetProperty("hero").ValueKind);
        Assert.Equal(JsonValueKind.Array, home.GetProperty("upcomingEvents").ValueKind);
        Assert.Equal(4, home.GetProperty("about").GetProperty("milestones").GetArrayLength());
    }

    [Fact]
    public async Task Admin_endpoints_require_authentication()
    {
        var response = await factory.Anonymous().GetAsync(Admin + "/news");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Sitemap_lists_public_pages()
    {
        var response = await factory.Anonymous().GetAsync("/sitemap.xml");
        var xml = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("https://stemdialogue.com/projects/school2university/about", xml);
    }

    [Fact]
    public async Task Unknown_project_returns_problem_details()
    {
        var response = await factory.Anonymous().GetAsync("/api/v1/projects/unknown");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Users_endpoint_is_admin_only_and_lists_the_bootstrap_admin()
    {
        var admin = await factory.AdminAsync();
        var users = await (await admin.GetAsync("/api/v1/admin/users")).JsonAsync();
        Assert.Contains(users.EnumerateArray(), u => u.Str("email") == ApiFactory.AdminEmail && u.Str("role") == "Admin");

        var editor = await (await admin.PostJsonAsync("/api/v1/admin/users", new
        {
            email = "editor@tests.local",
            fullName = "Editor",
            role = "Editor",
            password = "Editor-password-123",
        })).JsonAsync();
        Assert.Equal("Editor", editor.Str("role"));

        var editorClient = factory.Anonymous();
        var login = await (await editorClient.PostAsJsonAsync("/api/v1/auth/login", new { email = "editor@tests.local", password = "Editor-password-123" })).JsonAsync();
        editorClient.DefaultRequestHeaders.Authorization = new("Bearer", login.Str("accessToken"));

        Assert.Equal(HttpStatusCode.Forbidden, (await editorClient.GetAsync("/api/v1/admin/users")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await editorClient.GetAsync(Admin + "/news")).StatusCode);
    }
}
