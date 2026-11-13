using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StemDialogue.Application.Common;
using StemDialogue.Application.Identity;
using StemDialogue.Infrastructure.Persistence;

namespace StemDialogue.Infrastructure.Identity;

/// <summary>Issues short-lived JWT access tokens.</summary>
public sealed class TokenService(IOptions<JwtOptions> options, TimeProvider clock)
{
    public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var settings = options.Value;
        var now = clock.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(AppClaims.Subject, user.Id),
            new(AppClaims.Name, user.FullName),
            new(AppClaims.Email, user.Email ?? string.Empty),
            new(AppClaims.SecurityStamp, user.SecurityStamp ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };
        claims.AddRange(roles.Select(r => new Claim(AppClaims.Role, r)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
            IssuedAt = now,
            Expires = expires,
            SigningCredentials = new SigningCredentials(SigningKey(settings), SecurityAlgorithms.HmacSha256),
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }

    public static SymmetricSecurityKey SigningKey(JwtOptions settings) => new(Encoding.UTF8.GetBytes(settings.Key));

    public static string NewRefreshToken() => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(48));

    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}

public sealed class AuthService(
    UserManager<ApplicationUser> users,
    AppDbContext db,
    TokenService tokens,
    IOptions<JwtOptions> options,
    TimeProvider clock) : IAuthService
{
    /// <summary>Concurrent refreshes (two tabs) within this window are rejected without revoking the session.</summary>
    private static readonly TimeSpan RotationGrace = TimeSpan.FromSeconds(20);

    private const string InvalidCredentials = "Invalid email or password, or the account is locked.";

    public async Task<AuthSession> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive) throw new AuthenticationFailedException(InvalidCredentials);
        if (await users.IsLockedOutAsync(user)) throw new AuthenticationFailedException(InvalidCredentials);

        if (!await users.CheckPasswordAsync(user, request.Password))
        {
            await users.AccessFailedAsync(user);
            throw new AuthenticationFailedException(InvalidCredentials);
        }

        await users.ResetAccessFailedCountAsync(user);
        user.LastLoginAtUtc = clock.GetUtcNow().UtcDateTime;
        await users.UpdateAsync(user);

        return await IssueAsync(user, Guid.NewGuid(), ipAddress, ct);
    }

    public async Task<AuthSession> RefreshAsync(string refreshToken, string? ipAddress, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var hash = TokenService.Hash(refreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct)
            ?? throw new AuthenticationFailedException("The session has expired. Sign in again.");

        if (stored.RevokedAtUtc is not null)
        {
            var concurrentRotation = stored.ReplacedByHash is not null && now - stored.RevokedAtUtc < RotationGrace;
            if (!concurrentRotation && stored.ReplacedByHash is not null)
            {
                // A rotated token was presented again: treat as theft and end the whole session.
                await RevokeFamilyAsync(stored.FamilyId, "reuse-detected", now, ct);
                var owner = await users.FindByIdAsync(stored.UserId);
                if (owner is not null) await users.UpdateSecurityStampAsync(owner);
            }

            throw new AuthenticationFailedException("The session has expired. Sign in again.");
        }

        if (stored.ExpiresAtUtc <= now) throw new AuthenticationFailedException("The session has expired. Sign in again.");

        var user = await users.FindByIdAsync(stored.UserId);
        if (user is null || !user.IsActive) throw new AuthenticationFailedException("The account is not active.");

        var session = await IssueAsync(user, stored.FamilyId, ipAddress, ct, save: false);
        stored.RevokedAtUtc = now;
        stored.RevokedReason = "rotated";
        stored.ReplacedByHash = TokenService.Hash(session.RefreshToken);
        await db.SaveChangesAsync(ct);
        return session;
    }

    public async Task LogoutAsync(string? refreshToken, string? userId, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var hash = TokenService.Hash(refreshToken);
            var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
            if (stored is not null)
            {
                await RevokeFamilyAsync(stored.FamilyId, "logout", now, ct);
                userId ??= stored.UserId;
            }
        }

        if (userId is not null && await users.FindByIdAsync(userId) is { } user)
            await users.UpdateSecurityStampAsync(user); // invalidates access tokens already issued
    }

    public async Task<AuthUserDto> MeAsync(string userId, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId) ?? throw new AuthenticationFailedException("The account no longer exists.");
        return await ToDtoAsync(user);
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId) ?? throw new AuthenticationFailedException("The account no longer exists.");
        var result = await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded) throw IdentityErrors.ToValidation(result, nameof(request.NewPassword));

        // ChangePasswordAsync rotates the security stamp; end every refresh session too.
        await RevokeAllAsync(user.Id, "password-changed", clock.GetUtcNow().UtcDateTime, ct);
    }

    internal async Task RevokeAllAsync(string userId, string reason, DateTime now, CancellationToken ct)
    {
        var active = await db.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAtUtc == null).ToListAsync(ct);
        foreach (var token in active)
        {
            token.RevokedAtUtc = now;
            token.RevokedReason = reason;
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task RevokeFamilyAsync(Guid familyId, string reason, DateTime now, CancellationToken ct)
    {
        var family = await db.RefreshTokens.Where(t => t.FamilyId == familyId && t.RevokedAtUtc == null).ToListAsync(ct);
        foreach (var token in family)
        {
            token.RevokedAtUtc = now;
            token.RevokedReason = reason;
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task<AuthSession> IssueAsync(ApplicationUser user, Guid familyId, string? ipAddress, CancellationToken ct, bool save = true)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var roles = await users.GetRolesAsync(user);
        var (accessToken, accessExpires) = tokens.CreateAccessToken(user, roles);

        var refreshToken = TokenService.NewRefreshToken();
        var refreshExpires = now.AddDays(options.Value.RefreshTokenDays);
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = TokenService.Hash(refreshToken),
            FamilyId = familyId,
            CreatedAtUtc = now,
            ExpiresAtUtc = refreshExpires,
            CreatedByIp = ipAddress,
        });
        if (save) await db.SaveChangesAsync(ct);

        return new AuthSession(accessToken, accessExpires, refreshToken, refreshExpires, await ToDtoAsync(user, roles));
    }

    private async Task<AuthUserDto> ToDtoAsync(ApplicationUser user, IList<string>? roles = null) =>
        new(user.Id, user.Email ?? string.Empty, user.FullName, (roles ?? await users.GetRolesAsync(user)).ToList());
}

internal static class IdentityErrors
{
    public static Exception ToValidation(IdentityResult result, string field)
    {
        if (result.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName"))
            return new ConflictException("An account with this email already exists.");

        return new FluentValidation.ValidationException(
            result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(field, e.Description)));
    }
}
