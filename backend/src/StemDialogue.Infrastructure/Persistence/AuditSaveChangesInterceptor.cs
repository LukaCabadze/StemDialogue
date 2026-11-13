using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StemDialogue.Application.Abstractions;
using StemDialogue.Domain.Common;

namespace StemDialogue.Infrastructure.Persistence;

/// <summary>
/// Stamps authoring metadata, rotates concurrency stamps and writes an audit row for every
/// change to domain content, in the same transaction as the change itself.
/// </summary>
public sealed class AuditSaveChangesInterceptor(ICurrentUser currentUser, TimeProvider clock) : SaveChangesInterceptor
{
    private const int MaxValueLength = 300;

    private static readonly HashSet<string> IgnoredProperties =
    [
        nameof(AuditableEntity.CreatedAtUtc),
        nameof(AuditableEntity.CreatedBy),
        nameof(AuditableEntity.UpdatedAtUtc),
        nameof(AuditableEntity.UpdatedBy),
        nameof(AuditableEntity.ConcurrencyStamp),
    ];

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null) Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null) Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext context)
    {
        context.ChangeTracker.DetectChanges();
        var now = clock.GetUtcNow().UtcDateTime;
        var user = currentUser.IsAuthenticated ? currentUser.UserName ?? currentUser.UserId : "system";

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = now;
                    entry.Entity.CreatedBy = user;
                    entry.Entity.UpdatedAtUtc = now;
                    entry.Entity.UpdatedBy = user;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = now;
                    entry.Entity.UpdatedBy = user;
                    entry.Entity.ConcurrencyStamp = Guid.NewGuid();
                    break;
            }
        }

        var audit = context.ChangeTracker.Entries()
            .Where(e => e.Entity is Entity && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => ToAudit(e, now))
            .Where(a => a is not null)
            .ToList();

        context.Set<AuditEntry>().AddRange(audit!);
    }

    private AuditEntry? ToAudit(EntityEntry entry, DateTime now)
    {
        var action = entry.State switch
        {
            EntityState.Added => "Created",
            EntityState.Deleted => "Deleted",
            _ => "Updated",
        };

        Dictionary<string, string?>? changes = null;
        if (entry.State == EntityState.Modified)
        {
            changes = entry.Properties
                .Where(p => p.IsModified && !IgnoredProperties.Contains(p.Metadata.Name) && !Equals(p.OriginalValue, p.CurrentValue))
                .ToDictionary(p => p.Metadata.Name, p => Truncate(p.CurrentValue));

            // Bilingual values live in owned entries; report them as Title.Ka, Title.En …
            foreach (var reference in entry.References.Where(r => r.TargetEntry is { } t && t.Metadata.IsOwned()))
            {
                foreach (var property in reference.TargetEntry!.Properties.Where(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue)))
                    changes[$"{reference.Metadata.Name}.{property.Metadata.Name}"] = Truncate(property.CurrentValue);
            }

            if (changes.Count == 0 && entry.Entity is not AuditableEntity) return null;
        }

        return new AuditEntry
        {
            UserId = currentUser.UserId,
            UserName = currentUser.UserName,
            EntityType = entry.Metadata.ClrType.Name,
            EntityId = ((Entity)entry.Entity).Id.ToString(),
            Action = action,
            ChangesJson = changes is { Count: > 0 } ? JsonSerializer.Serialize(changes) : null,
            OccurredAtUtc = now,
        };
    }

    private static string? Truncate(object? value)
    {
        var text = value switch
        {
            null => null,
            DateTime d => d.ToString("O"),
            _ => value.ToString(),
        };
        return text is { Length: > MaxValueLength } ? text[..MaxValueLength] + "…" : text;
    }
}
