using System.Security.Claims;
using Microsoft.AspNetCore.OutputCaching;
using StemDialogue.Application.Abstractions;
using StemDialogue.Domain.Common;
using StemDialogue.Infrastructure.Identity;

namespace StemDialogue.Api.Web;

internal sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public string? UserId => Principal?.FindFirstValue(AppClaims.Subject);
    public string? UserName => Principal?.FindFirstValue(AppClaims.Email) ?? Principal?.FindFirstValue(AppClaims.Name);
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;
    public bool IsInRole(string role) => Principal?.IsInRole(role) == true;
}

/// <summary>Public content language from <c>?lang=en</c>; Georgian by default (spec: Georgian first, English later).</summary>
internal sealed class RequestLanguageContext(IHttpContextAccessor accessor) : ILanguageContext
{
    public string Language =>
        string.Equals(accessor.HttpContext?.Request.Query["lang"], LocalizedText.English, StringComparison.OrdinalIgnoreCase)
            ? LocalizedText.English
            : LocalizedText.Georgian;
}

internal static class CachePolicies
{
    public const string Public = "public";
    public const string Tag = "public-content";
}

internal sealed class OutputCacheInvalidator(IOutputCacheStore store) : IPublicCacheInvalidator
{
    public Task InvalidateAsync(CancellationToken ct) => store.EvictByTagAsync(CachePolicies.Tag, ct).AsTask();
}
