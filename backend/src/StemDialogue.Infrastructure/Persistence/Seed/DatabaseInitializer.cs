using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StemDialogue.Application.Identity;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;
using StemDialogue.Domain.News;
using StemDialogue.Domain.People;
using StemDialogue.Domain.Resources;
using StemDialogue.Infrastructure.Identity;

namespace StemDialogue.Infrastructure.Persistence.Seed;

/// <summary>
/// Applies migrations (SQL Server) or creates the schema (SQLite), then seeds roles, the bootstrap
/// administrator and the confirmed project content. Idempotent; run with <c>--migrate</c>.
/// </summary>
public sealed class DatabaseInitializer(
    AppDbContext db,
    UserManager<ApplicationUser> users,
    RoleManager<IdentityRole> roles,
    IConfiguration configuration,
    TimeProvider clock,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(bool includeDemoContent, CancellationToken ct = default)
    {
        if (db.Database.IsSqlServer())
            await db.Database.MigrateAsync(ct);
        else
            await db.Database.EnsureCreatedAsync(ct);

        await SeedRolesAsync();
        await SeedBootstrapAdminAsync();
        await SeedProjectAsync(ct);
        if (includeDemoContent) await SeedDemoContentAsync(ct);
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in AppRoles.All)
        {
            if (await roles.RoleExistsAsync(role)) continue;
            var result = await roles.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded) throw new InvalidOperationException($"Could not create role {role}.");
        }
    }

    /// <summary>Creates the first administrator from Bootstrap__Email / Bootstrap__Password when no account exists.</summary>
    private async Task SeedBootstrapAdminAsync()
    {
        var email = configuration["Bootstrap:Email"];
        var password = configuration["Bootstrap:Password"];
        if (string.IsNullOrWhiteSpace(email) || await users.Users.AnyAsync()) return;
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Bootstrap__Password is required together with Bootstrap__Email.");

        var admin = new ApplicationUser
        {
            UserName = email.Trim(),
            Email = email.Trim(),
            EmailConfirmed = true,
            FullName = configuration["Bootstrap:FullName"] ?? "Administrator",
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
            LockoutEnabled = true,
        };

        var created = await users.CreateAsync(admin, password);
        if (!created.Succeeded)
            throw new InvalidOperationException("Bootstrap administrator: " + string.Join(" ", created.Errors.Select(e => e.Description)));
        await users.AddToRoleAsync(admin, AppRoles.Admin);
        logger.LogInformation("Bootstrap administrator {Email} created.", admin.Email);
    }

    private async Task SeedProjectAsync(CancellationToken ct)
    {
        if (await db.Projects.AnyAsync(p => p.Slug == School2UniversitySeed.Slug, ct)) return;

        var (funder, btu, atsu) = School2UniversitySeed.Organizations();
        db.Organizations.AddRange(funder, btu, atsu);

        var project = School2UniversitySeed.Project(funder, btu, atsu);
        db.Projects.Add(project);
        db.Pages.AddRange(School2UniversitySeed.Pages(project.Id));
        db.Milestones.AddRange(School2UniversitySeed.Milestones(project.Id));
        if (!await db.Disciplines.AnyAsync(ct)) db.Disciplines.AddRange(School2UniversitySeed.Disciplines());
        db.People.AddRange(School2UniversitySeed.CoreTeam(project.Id));

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded project {Slug}.", project.Slug);
    }

    /// <summary>Clearly marked sample content for local development only.</summary>
    private async Task SeedDemoContentAsync(CancellationToken ct)
    {
        var project = await db.Projects.FirstAsync(p => p.Slug == School2UniversitySeed.Slug, ct);
        if (await db.Events.AnyAsync(e => e.ProjectId == project.Id, ct)) return;

        var now = clock.GetUtcNow().UtcDateTime;
        var physics = await db.Disciplines.FirstAsync(d => d.Slug == "mathematics-physics", ct);

        var researcher = new Person
        {
            Slug = "demo-researcher",
            FullName = new LocalizedText("[დემო] ნინო მკვლევარიძე", "[Demo] Nino Researcher"),
            AcademicTitle = new LocalizedText("ფიზიკის დოქტორი", "PhD in Physics"),
            Organization = new LocalizedText("ბიზნესისა და ტექნოლოგიების უნივერსიტეტი", "Business and Technology University"),
            ShortBio = new LocalizedText("სადემონსტრაციო პროფილი — მკვლევარიც და მენტორიც ერთ პროფილში.", "Demo profile: researcher and mentor in one profile."),
            Status = PublicationStatus.Published,
        };
        researcher.Disciplines.Add(physics);
        researcher.Roles.Add(new PersonRoleAssignment { PersonId = researcher.Id, ProjectId = project.Id, Role = PersonRole.Researcher, StartDate = DateOnly.FromDateTime(now) });
        researcher.Roles.Add(new PersonRoleAssignment { PersonId = researcher.Id, ProjectId = project.Id, Role = PersonRole.Mentor, StartDate = DateOnly.FromDateTime(now) });
        db.People.Add(researcher);

        var nextSaturday = now.Date.AddDays(((int)DayOfWeek.Saturday - (int)now.DayOfWeek + 7) % 7 + 7).AddHours(8); // 12:00 Tbilisi
        var upcoming = new Event
        {
            ProjectId = project.Id,
            Slug = "demo-stem-saturday-physics",
            Title = new LocalizedText("[დემო] STEM შაბათობა: ფიზიკა ყოველდღიურობაში", "[Demo] STEM Saturday: physics in everyday life"),
            Summary = new LocalizedText("ონლაინ შეხვედრა მკვლევართან.", "An online meeting with a researcher."),
            Description = new LocalizedText("<p>სადემონსტრაციო ანონსი.</p>", "<p>Demo announcement.</p>"),
            StartsAtUtc = nextSaturday,
            EndsAtUtc = nextSaturday.AddHours(2),
            Format = EventFormat.Online,
            RegistrationUrl = "https://forms.gle/example",
            Publication = PublicationStatus.Published,
            FirstPublishedAtUtc = now,
            IsFeatured = true,
        };
        upcoming.Disciplines.Add(physics);
        upcoming.Participants.Add(new EventParticipant { EventId = upcoming.Id, PersonId = researcher.Id, Role = EventParticipantRole.Speaker });

        var lastMonth = nextSaturday.AddDays(-35);
        var completed = new Event
        {
            ProjectId = project.Id,
            Slug = "demo-stem-saturday-robotics",
            Title = new LocalizedText("[დემო] STEM შაბათობა: რობოტიკა", "[Demo] STEM Saturday: robotics"),
            Summary = new LocalizedText("ჩატარებული შეხვედრის მაგალითი არქივში.", "Example of a past meeting in the archive."),
            StartsAtUtc = lastMonth,
            EndsAtUtc = lastMonth.AddHours(2),
            Format = EventFormat.Online,
            Status = EventStatus.Completed,
            CompletedAtUtc = lastMonth.AddHours(2),
            Recap = new LocalizedText("<p>შეხვედრის შეჯამება, ჩანაწერი და მასალები ერთ გვერდზე.</p>", "<p>Summary, recording and materials on one page.</p>"),
            RecordingUrl = "https://www.youtube.com/watch?v=DEMO0000001",
            Publication = PublicationStatus.Published,
            FirstPublishedAtUtc = lastMonth.AddDays(-7),
        };
        completed.Participants.Add(new EventParticipant { EventId = completed.Id, PersonId = researcher.Id, Role = EventParticipantRole.Mentor });
        db.Events.AddRange(upcoming, completed);

        db.Resources.Add(new Resource
        {
            ProjectId = project.Id,
            Slug = "demo-robotics-recording",
            Title = new LocalizedText("[დემო] რობოტიკის შეხვედრის ჩანაწერი", "[Demo] Robotics meeting recording"),
            Type = ResourceType.Video,
            ExternalUrl = "https://www.youtube.com/watch?v=DEMO0000001",
            EventId = completed.Id,
            AuthorId = researcher.Id,
            Status = PublicationStatus.Published,
            PublishedAtUtc = lastMonth.AddDays(1),
        });

        db.News.Add(new NewsArticle
        {
            ProjectId = project.Id,
            Slug = "demo-open-call",
            Title = new LocalizedText("[დემო] ღია მოწვევა IX–XII კლასის მოსწავლეებს", "[Demo] Open call for grade 9–12 students"),
            Summary = new LocalizedText("სადემონსტრაციო განცხადება.", "Demo announcement."),
            Body = new LocalizedText("<p>სადემონსტრაციო ტექსტი.</p>", "<p>Demo text.</p>"),
            Category = NewsCategory.Opportunity,
            Status = PublicationStatus.Published,
            PublishAtUtc = now.AddDays(-2),
            FirstPublishedAtUtc = now.AddDays(-2),
            RelatedEventId = upcoming.Id,
        });

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded development demo content.");
    }
}
