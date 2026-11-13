namespace StemDialogue.Application.Abstractions;

/// <summary>The authenticated administrator or editor performing a request.</summary>
public interface ICurrentUser
{
    string? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

/// <summary>Language of the current public request: <c>ka</c> (default) or <c>en</c>.</summary>
public interface ILanguageContext
{
    string Language { get; }
}

/// <summary>Binary storage for uploaded media.</summary>
public interface IFileStorage
{
    /// <summary>Stores <paramref name="content"/> under <paramref name="relativePath"/>.</summary>
    Task SaveAsync(Stream content, string relativePath, CancellationToken ct);

    Task DeleteAsync(string relativePath, CancellationToken ct);

    /// <summary>Public URL for a stored file (relative to the site unless a CDN base is configured).</summary>
    string GetPublicUrl(string relativePath);
}

/// <summary>Removes unsafe markup from editor-supplied rich text.</summary>
public interface IHtmlContentSanitizer
{
    string Sanitize(string? html);
}

/// <summary>Removes cached public responses after content changes.</summary>
public interface IPublicCacheInvalidator
{
    Task InvalidateAsync(CancellationToken ct);
}
