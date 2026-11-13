using Microsoft.EntityFrameworkCore;
using StemDialogue.Domain.Ambassadors;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;
using StemDialogue.Domain.Media;
using StemDialogue.Domain.News;
using StemDialogue.Domain.Organizations;
using StemDialogue.Domain.Pages;
using StemDialogue.Domain.People;
using StemDialogue.Domain.Projects;
using StemDialogue.Domain.Resources;
using StemDialogue.Domain.Taxonomy;

namespace StemDialogue.Application.Abstractions;

/// <summary>Persistence boundary used by application services. Implemented by the EF Core context.</summary>
public interface IAppDbContext
{
    DbSet<Project> Projects { get; }
    DbSet<ProjectPartner> ProjectPartners { get; }
    DbSet<ProjectMilestone> Milestones { get; }
    DbSet<SocialLink> SocialLinks { get; }
    DbSet<Organization> Organizations { get; }
    DbSet<NewsArticle> News { get; }
    DbSet<Event> Events { get; }
    DbSet<EventParticipant> EventParticipants { get; }
    DbSet<Person> People { get; }
    DbSet<PersonRoleAssignment> PersonRoles { get; }
    DbSet<Resource> Resources { get; }
    DbSet<SchoolActivity> SchoolActivities { get; }
    DbSet<SitePage> Pages { get; }
    DbSet<MediaAsset> Media { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Discipline> Disciplines { get; }
    DbSet<SlugRedirect> SlugRedirects { get; }
    DbSet<AuditEntry> AuditEntries { get; }

    /// <summary>Tells the change tracker which stamp the client last saw, so a concurrent save fails with 409.</summary>
    void UseClientConcurrencyStamp(AuditableEntity entity, Guid clientStamp);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
