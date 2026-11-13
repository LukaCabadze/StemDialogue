using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StemDialogue.Infrastructure.Identity;

namespace StemDialogue.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.Property(u => u.FullName).HasMaxLength(150).IsRequired();
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("RefreshTokens");
        b.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        b.HasIndex(t => t.TokenHash).IsUnique();
        b.HasIndex(t => t.FamilyId);
        b.Property(t => t.ReplacedByHash).HasMaxLength(64);
        b.Property(t => t.RevokedReason).HasMaxLength(60);
        b.Property(t => t.CreatedByIp).HasMaxLength(64);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
