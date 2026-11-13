namespace StemDialogue.Application.Common;

/// <summary>HTTP 404.</summary>
public sealed class NotFoundException(string resource, object key)
    : Exception($"{resource} '{key}' was not found.")
{
    public string Resource { get; } = resource;
}

/// <summary>HTTP 409: duplicate value, stale concurrency stamp or record still in use.</summary>
public sealed class ConflictException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>HTTP 401: invalid credentials, locked account or invalid/expired refresh session.</summary>
public sealed class AuthenticationFailedException(string message) : Exception(message);

/// <summary>HTTP 403: authenticated but not allowed (for example demoting the last administrator).</summary>
public sealed class ForbiddenException(string message) : Exception(message);

/// <summary>
/// HTTP 301 for a public detail URL whose slug was renamed. <see cref="NewSlug"/> is the canonical slug.
/// </summary>
public sealed class SlugMovedException(string newSlug) : Exception($"Moved to '{newSlug}'.")
{
    public string NewSlug { get; } = newSlug;
}
