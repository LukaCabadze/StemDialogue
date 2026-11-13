using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StemDialogue.Application.Admin;
using StemDialogue.Application.Ambassadors;
using StemDialogue.Application.Common;
using StemDialogue.Application.Events;
using StemDialogue.Application.Home;
using StemDialogue.Application.Media;
using StemDialogue.Application.News;
using StemDialogue.Application.Organizations;
using StemDialogue.Application.Pages;
using StemDialogue.Application.People;
using StemDialogue.Application.Projects;
using StemDialogue.Application.Resources;
using StemDialogue.Application.Sitemap;
using StemDialogue.Application.Taxonomy;

namespace StemDialogue.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        // Shared building blocks
        services.AddScoped<ProjectResolver>();
        services.AddScoped<PublicMapper>();
        services.AddScoped<ContentCards>();
        services.AddScoped<ContentReferences>();
        services.AddScoped<SlugManager>();

        // Public site
        services.AddScoped<SiteService>();
        services.AddScoped<HomeService>();
        services.AddScoped<PublicPageService>();
        services.AddScoped<PublicNewsService>();
        services.AddScoped<PublicEventService>();
        services.AddScoped<PublicPeopleService>();
        services.AddScoped<PublicResourceService>();
        services.AddScoped<PublicAmbassadorService>();
        services.AddScoped<SitemapService>();

        // CMS
        services.AddScoped<ProjectAdminService>();
        services.AddScoped<PageAdminService>();
        services.AddScoped<MilestoneAdminService>();
        services.AddScoped<NewsAdminService>();
        services.AddScoped<EventAdminService>();
        services.AddScoped<PeopleAdminService>();
        services.AddScoped<ResourceAdminService>();
        services.AddScoped<SchoolActivityAdminService>();
        services.AddScoped<OrganizationAdminService>();
        services.AddScoped<TaxonomyAdminService>();
        services.AddScoped<MediaService>();
        services.AddScoped<AdminOverviewService>();

        return services;
    }
}
