using Microsoft.AspNetCore.Identity;

namespace StemDialogue.Infrastructure.Identity;

/// <summary>CMS account (administrator or editor). Public visitors never have accounts.</summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
}

/// <summary>
/// Rotating refresh token. Only a SHA-256 hash is stored. Tokens issued from one login share a
/// <see cref="FamilyId"/>; reusing a rotated token revokes the family.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokedReason { get; set; }
    public string? ReplacedByHash { get; set; }
    public string? CreatedByIp { get; set; }
}

public sealed class JwtOptions
{
    public const string Section = "Jwt";

    public string Issuer { get; set; } = "StemDialogue";
    public string Audience { get; set; } = "StemDialogue";

    /// <summary>HMAC-SHA256 signing secret, at least 32 bytes. Supplied through configuration/environment only.</summary>
    public string Key { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 14;
}

/// <summary>JWT claim names used by the API (inbound claim mapping is disabled).</summary>
public static class AppClaims
{
    public const string Subject = "sub";
    public const string Name = "name";
    public const string Email = "email";
    public const string Role = "role";
    public const string SecurityStamp = "stamp";
}
