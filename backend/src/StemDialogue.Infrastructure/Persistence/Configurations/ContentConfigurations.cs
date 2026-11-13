using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StemDialogue.Domain.Ambassadors;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;
using StemDialogue.Domain.Media;
using StemDialogue.Domain.News;
using StemDialogue.Domain.People;
using StemDialogue.Domain.Projects;
using StemDialogue.Domain.Resources;
using StemDialogue.Domain.Taxonomy;

namespace StemDialogue.Infrastructure.Persistence.Configurations;

internal sealed class NewsConfiguration : IEntityTypeConfiguration<NewsArticle>
{
    public void Configure(EntityTypeBuilder<NewsArticle> b)
    {
        b.ToTable("News");
        b.Property(n => n.Slug).HasMaxLength(Lengths.Slug).IsRequired();
        b.HasIndex(n => new { n.ProjectId, n.Slug }).IsUnique();
        b.HasIndex(n => new { n.ProjectId, n.Status, n.PublishAtUtc });

        b.HasOne<Project>().WithMany().HasForeignKey(n => n.ProjectId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(n => n.CoverImage).WithMany().HasForeignKey(n => n.CoverImageId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(n => n.ShareImage).WithMany().HasForeignKey(n => n.ShareImageId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(n => n.RelatedEvent).WithMany().HasForeignKey(n => n.RelatedEventId).OnDelete(DeleteBehavior.SetNull);

        b.HasMany(n => n.Tags).WithMany().UsingEntity("NewsTags");
        b.HasMany(n => n.Gallery).WithOne().HasForeignKey(g => g.NewsArticleId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class NewsGalleryConfiguration : IEntityTypeConfiguration<NewsGalleryItem>
{
    public void Configure(EntityTypeBuilder<NewsGalleryItem> b)
    {
        b.ToTable("NewsGallery");
        b.HasKey(g => new { g.NewsArticleId, g.MediaAssetId });
        b.HasOne(g => g.MediaAsset).WithMany().HasForeignKey(g => g.MediaAssetId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> b)
    {
        b.ToTable("Events");
        b.Property(e => e.Slug).HasMaxLength(Lengths.Slug).IsRequired();
        b.HasIndex(e => new { e.ProjectId, e.Slug }).IsUnique();
        b.HasIndex(e => new { e.ProjectId, e.Publication, e.StartsAtUtc });
        b.Property(e => e.StreamUrl).HasMaxLength(Lengths.Url);
        b.Property(e => e.RegistrationUrl).HasMaxLength(Lengths.Url);
        b.Property(e => e.RecordingUrl).HasMaxLength(Lengths.Url);

        b.HasOne<Project>().WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.CoverImage).WithMany().HasForeignKey(e => e.CoverImageId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.ShareImage).WithMany().HasForeignKey(e => e.ShareImageId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(e => e.Tags).WithMany().UsingEntity("EventTags");
        b.HasMany(e => e.Disciplines).WithMany().UsingEntity("EventDisciplines");
        b.HasMany(e => e.Participants).WithOne().HasForeignKey(p => p.EventId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(e => e.Gallery).WithOne().HasForeignKey(g => g.EventId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(e => e.Resources).WithOne(r => r.Event).HasForeignKey(r => r.EventId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class EventParticipantConfiguration : IEntityTypeConfiguration<EventParticipant>
{
    public void Configure(EntityTypeBuilder<EventParticipant> b)
    {
        b.ToTable("EventParticipants");
        b.HasKey(p => new { p.EventId, p.PersonId });
        b.HasOne(p => p.Person).WithMany(p => p.Events).HasForeignKey(p => p.PersonId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class EventGalleryConfiguration : IEntityTypeConfiguration<EventGalleryItem>
{
    public void Configure(EntityTypeBuilder<EventGalleryItem> b)
    {
        b.ToTable("EventGallery");
        b.HasKey(g => new { g.EventId, g.MediaAssetId });
        b.HasOne(g => g.MediaAsset).WithMany().HasForeignKey(g => g.MediaAssetId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> b)
    {
        b.ToTable("People");
        b.Property(p => p.Slug).HasMaxLength(Lengths.Slug).IsRequired();
        b.HasIndex(p => p.Slug).IsUnique();
        b.Property(p => p.Email).HasMaxLength(Lengths.Email);

        b.HasOne(p => p.Photo).WithMany().HasForeignKey(p => p.PhotoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(p => p.ShareImage).WithMany().HasForeignKey(p => p.ShareImageId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(p => p.Roles).WithOne().HasForeignKey(r => r.PersonId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(p => p.Links).WithOne().HasForeignKey(l => l.PersonId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(p => p.Disciplines).WithMany().UsingEntity("PersonDisciplines");
    }
}

internal sealed class PersonRoleConfiguration : IEntityTypeConfiguration<PersonRoleAssignment>
{
    public void Configure(EntityTypeBuilder<PersonRoleAssignment> b)
    {
        b.ToTable("PersonRoles");

        // One profile, many roles; but each role only once per project (spec §7.2, §15.18).
        b.HasIndex(r => new { r.PersonId, r.ProjectId, r.Role }).IsUnique();
        b.HasIndex(r => new { r.ProjectId, r.Role });
        b.HasOne<Project>().WithMany().HasForeignKey(r => r.ProjectId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PersonLinkConfiguration : IEntityTypeConfiguration<PersonLink>
{
    public void Configure(EntityTypeBuilder<PersonLink> b)
    {
        b.ToTable("PersonLinks");
        b.Property(l => l.Url).HasMaxLength(Lengths.Url).IsRequired();
    }
}

internal sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> b)
    {
        b.ToTable("Resources");
        b.Property(r => r.Slug).HasMaxLength(Lengths.Slug).IsRequired();
        b.HasIndex(r => new { r.ProjectId, r.Slug }).IsUnique();
        b.HasIndex(r => new { r.ProjectId, r.Status, r.Type });
        b.Property(r => r.ExternalUrl).HasMaxLength(Lengths.Url);

        b.HasOne<Project>().WithMany().HasForeignKey(r => r.ProjectId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(r => r.File).WithMany().HasForeignKey(r => r.FileId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(r => r.Thumbnail).WithMany().HasForeignKey(r => r.ThumbnailId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(r => r.Author).WithMany().HasForeignKey(r => r.AuthorId).OnDelete(DeleteBehavior.SetNull);
        b.HasMany(r => r.Tags).WithMany().UsingEntity("ResourceTags");
        b.HasMany(r => r.Disciplines).WithMany().UsingEntity("ResourceDisciplines");
    }
}

internal sealed class SchoolActivityConfiguration : IEntityTypeConfiguration<SchoolActivity>
{
    public void Configure(EntityTypeBuilder<SchoolActivity> b)
    {
        b.ToTable("SchoolActivities");
        b.Property(a => a.Slug).HasMaxLength(Lengths.Slug).IsRequired();
        b.HasIndex(a => new { a.ProjectId, a.Slug }).IsUnique();
        b.Property(a => a.ConsentReference).HasMaxLength(300);
        b.Ignore(a => a.HasImagery);

        b.HasOne<Project>().WithMany().HasForeignKey(a => a.ProjectId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(a => a.CoverImage).WithMany().HasForeignKey(a => a.CoverImageId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(a => a.Gallery).WithOne().HasForeignKey(g => g.SchoolActivityId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SchoolActivityGalleryConfiguration : IEntityTypeConfiguration<SchoolActivityGalleryItem>
{
    public void Configure(EntityTypeBuilder<SchoolActivityGalleryItem> b)
    {
        b.ToTable("SchoolActivityGallery");
        b.HasKey(g => new { g.SchoolActivityId, g.MediaAssetId });
        b.HasOne(g => g.MediaAsset).WithMany().HasForeignKey(g => g.MediaAssetId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class MediaConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> b)
    {
        b.ToTable("Media");
        b.Property(m => m.StoragePath).HasMaxLength(300).IsRequired();
        b.HasIndex(m => m.StoragePath).IsUnique();
        b.Property(m => m.OriginalFileName).HasMaxLength(255).IsRequired();
        b.Property(m => m.ContentType).HasMaxLength(120).IsRequired();
        b.Property(m => m.Sha256).HasMaxLength(64).IsRequired();
        b.HasIndex(m => m.Sha256);
        b.HasIndex(m => m.CreatedAtUtc);
    }
}

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> b)
    {
        b.ToTable("Tags");
        b.Property(t => t.Slug).HasMaxLength(Lengths.Slug).IsRequired();
        b.HasIndex(t => t.Slug).IsUnique();
    }
}

internal sealed class DisciplineConfiguration : IEntityTypeConfiguration<Discipline>
{
    public void Configure(EntityTypeBuilder<Discipline> b)
    {
        b.ToTable("Disciplines");
        b.Property(d => d.Slug).HasMaxLength(Lengths.Slug).IsRequired();
        b.HasIndex(d => d.Slug).IsUnique();
    }
}

internal sealed class SlugRedirectConfiguration : IEntityTypeConfiguration<SlugRedirect>
{
    public void Configure(EntityTypeBuilder<SlugRedirect> b)
    {
        b.ToTable("SlugRedirects");
        b.Property(r => r.OldSlug).HasMaxLength(Lengths.Slug).IsRequired();

        // Includes rows with a null ProjectId (people), so the default SQL Server null filter is removed.
        b.HasIndex(r => new { r.ContentType, r.ProjectId, r.OldSlug }).IsUnique().HasFilter(null);
    }
}

internal sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> b)
    {
        b.ToTable("AuditLog");
        b.HasKey(a => a.Id);
        b.Property(a => a.UserId).HasMaxLength(450);
        b.Property(a => a.UserName).HasMaxLength(256);
        b.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
        b.Property(a => a.EntityId).HasMaxLength(100).IsRequired();
        b.Property(a => a.Action).HasMaxLength(20).IsRequired();
        b.HasIndex(a => a.OccurredAtUtc);
        b.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}
