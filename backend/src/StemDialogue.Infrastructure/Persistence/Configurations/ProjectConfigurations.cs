using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StemDialogue.Domain.Organizations;
using StemDialogue.Domain.Pages;
using StemDialogue.Domain.Projects;

namespace StemDialogue.Infrastructure.Persistence.Configurations;

internal static class Lengths
{
    public const int Slug = 120;
    public const int Url = 2048;
    public const int Email = 254;
    public const int Phone = 40;
}

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.ToTable("Projects");
        b.HasIndex(p => p.Slug).IsUnique();
        b.Property(p => p.Slug).HasMaxLength(Lengths.Slug).IsRequired();
        b.Property(p => p.Code).HasMaxLength(40).IsRequired();
        b.Property(p => p.ContactEmail).HasMaxLength(Lengths.Email);
        b.Property(p => p.ContactPhone).HasMaxLength(Lengths.Phone);

        b.HasOne(p => p.Logo).WithMany().HasForeignKey(p => p.LogoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(p => p.ShareImage).WithMany().HasForeignKey(p => p.ShareImageId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(p => p.Partners).WithOne().HasForeignKey(p => p.ProjectId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(p => p.SocialLinks).WithOne().HasForeignKey(s => s.ProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ProjectPartnerConfiguration : IEntityTypeConfiguration<ProjectPartner>
{
    public void Configure(EntityTypeBuilder<ProjectPartner> b)
    {
        b.ToTable("ProjectPartners");
        b.HasKey(p => new { p.ProjectId, p.OrganizationId });
        b.HasOne(p => p.Organization).WithMany().HasForeignKey(p => p.OrganizationId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    public void Configure(EntityTypeBuilder<SocialLink> b)
    {
        b.ToTable("SocialLinks");
        b.Property(s => s.Url).HasMaxLength(Lengths.Url).IsRequired();
    }
}

internal sealed class MilestoneConfiguration : IEntityTypeConfiguration<ProjectMilestone>
{
    public void Configure(EntityTypeBuilder<ProjectMilestone> b)
    {
        b.ToTable("ProjectMilestones");
        b.HasOne<Project>().WithMany().HasForeignKey(m => m.ProjectId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(m => new { m.ProjectId, m.DisplayOrder });
    }
}

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> b)
    {
        b.ToTable("Organizations");
        b.Property(o => o.WebsiteUrl).HasMaxLength(Lengths.Url);
        b.HasOne(o => o.Logo).WithMany().HasForeignKey(o => o.LogoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(o => o.LogoOnDark).WithMany().HasForeignKey(o => o.LogoOnDarkId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SitePageConfiguration : IEntityTypeConfiguration<SitePage>
{
    public void Configure(EntityTypeBuilder<SitePage> b)
    {
        b.ToTable("SitePages");
        b.Property(p => p.Key).HasMaxLength(40).IsRequired();
        b.HasIndex(p => new { p.ProjectId, p.Key }).IsUnique();
        b.Property(p => p.PrimaryActionUrl).HasMaxLength(Lengths.Url);
        b.Property(p => p.SecondaryActionUrl).HasMaxLength(Lengths.Url);
        b.HasOne<Project>().WithMany().HasForeignKey(p => p.ProjectId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(p => p.HeroImage).WithMany().HasForeignKey(p => p.HeroImageId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(p => p.ShareImage).WithMany().HasForeignKey(p => p.ShareImageId).OnDelete(DeleteBehavior.Restrict);
    }
}
