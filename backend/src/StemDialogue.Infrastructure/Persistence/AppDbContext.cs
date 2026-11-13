using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
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
using StemDialogue.Infrastructure.Identity;

namespace StemDialogue.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options), IAppDbContext
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectPartner> ProjectPartners => Set<ProjectPartner>();
    public DbSet<ProjectMilestone> Milestones => Set<ProjectMilestone>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<NewsArticle> News => Set<NewsArticle>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<PersonRoleAssignment> PersonRoles => Set<PersonRoleAssignment>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<SchoolActivity> SchoolActivities => Set<SchoolActivity>();
    public DbSet<SitePage> Pages => Set<SitePage>();
    public DbSet<MediaAsset> Media => Set<MediaAsset>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Discipline> Disciplines => Set<Discipline>();
    public DbSet<SlugRedirect> SlugRedirects => Set<SlugRedirect>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public void UseClientConcurrencyStamp(AuditableEntity entity, Guid clientStamp)
    {
        var entry = Entry(entity);
        entry.Property(e => e.ConcurrencyStamp).OriginalValue = clientStamp;

        // An update always counts as a modification, even when only owned values or child collections changed,
        // so the stamp rotates and UpdatedAt/UpdatedBy are recorded.
        entry.Property(e => e.UpdatedAtUtc).IsModified = true;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("This item was changed by someone else. Reload it and apply your edits again.", ex);
        }
        catch (DbUpdateException ex) when (DatabaseErrors.IsUniqueViolation(ex))
        {
            throw new ConflictException("A record with the same unique value already exists.", ex);
        }
        catch (DbUpdateException ex) when (DatabaseErrors.IsReferenceViolation(ex))
        {
            throw new ConflictException("This record is referenced by other content and cannot be changed this way.", ex);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Owned<LocalizedText>();
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        var utc = new UtcDateTimeConverter();
        foreach (var entityType in builder.Model.GetEntityTypes().ToList())
        {
            // Every timestamp is UTC; restore DateTimeKind.Utc on read so JSON carries a 'Z' suffix.
            foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
                property.SetValueConverter(utc);

            // Bilingual values are mandatory parts of their owner (two columns on the owner's table).
            foreach (var navigation in entityType.GetNavigations().Where(n => n.TargetEntityType.ClrType == typeof(LocalizedText)))
                navigation.ForeignKey.IsRequiredDependent = true;

            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType) && !entityType.IsOwned())
            {
                builder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.ConcurrencyStamp)).IsConcurrencyToken();
                builder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.CreatedBy)).HasMaxLength(256);
                builder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.UpdatedBy)).HasMaxLength(256);
            }
        }
    }

}

internal sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : DateTime.SpecifyKind(v, DateTimeKind.Utc),
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

internal static class DatabaseErrors
{
    public static bool IsUniqueViolation(DbUpdateException ex) => ex.InnerException switch
    {
        Microsoft.Data.SqlClient.SqlException sql => sql.Number is 2601 or 2627,
        { } other => other.Message.Contains("UNIQUE constraint failed", StringComparison.Ordinal),
        _ => false,
    };

    public static bool IsReferenceViolation(DbUpdateException ex) => ex.InnerException switch
    {
        Microsoft.Data.SqlClient.SqlException sql => sql.Number == 547,
        { } other => other.Message.Contains("FOREIGN KEY constraint failed", StringComparison.Ordinal),
        _ => false,
    };
}
