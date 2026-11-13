using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StemDialogue.Api.Web;
using StemDialogue.Application.Common;
using StemDialogue.Application.Identity;
using StemDialogue.Infrastructure.Identity;

namespace StemDialogue.Api.Controllers;

/// <summary>
/// CMS sign-in. The access token is returned in the body (keep it in memory); the rotating refresh token
/// is an HttpOnly, Secure, SameSite=Strict cookie scoped to this controller.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    private const string RefreshCookie = "sd_refresh";
    private const string CookiePath = "/api/v1/auth";

    /// <summary>Signs in with email and password.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimits.Auth)]
    [ProducesResponseType<AccessTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<AccessTokenResponse> Login(LoginRequest request, CancellationToken ct)
    {
        var session = await auth.LoginAsync(request, ClientIp, ct);
        SetRefreshCookie(session);
        return new AccessTokenResponse(session.AccessToken, session.AccessTokenExpiresAtUtc, session.User);
    }

    /// <summary>Issues a new access token using the refresh cookie (rotates the cookie).</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimits.Auth)]
    public async Task<AccessTokenResponse> Refresh(CancellationToken ct)
    {
        var token = Request.Cookies[RefreshCookie];
        if (string.IsNullOrEmpty(token)) throw new AuthenticationFailedException("Sign in to continue.");

        try
        {
            var session = await auth.RefreshAsync(token, ClientIp, ct);
            SetRefreshCookie(session);
            return new AccessTokenResponse(session.AccessToken, session.AccessTokenExpiresAtUtc, session.User);
        }
        catch (AuthenticationFailedException)
        {
            DeleteRefreshCookie();
            throw;
        }
    }

    /// <summary>Ends the session: revokes the refresh token and invalidates issued access tokens.</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = User.Identity?.IsAuthenticated == true ? User.FindFirst(AppClaims.Subject)?.Value : null;
        await auth.LogoutAsync(Request.Cookies[RefreshCookie], userId, ct);
        DeleteRefreshCookie();
        return NoContent();
    }

    /// <summary>The signed-in account.</summary>
    [HttpGet("me")]
    [Authorize]
    public Task<AuthUserDto> Me(CancellationToken ct) => auth.MeAsync(UserId, ct);

    /// <summary>Changes the password. All sessions end; sign in again afterwards.</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        await auth.ChangePasswordAsync(UserId, request, ct);
        DeleteRefreshCookie();
        return NoContent();
    }

    private string UserId => User.FindFirst(AppClaims.Subject)?.Value
        ?? throw new AuthenticationFailedException("Sign in to continue.");

    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    private void SetRefreshCookie(AuthSession session) =>
        Response.Cookies.Append(RefreshCookie, session.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = CookiePath,
            Expires = session.RefreshTokenExpiresAtUtc,
            IsEssential = true,
        });

    private void DeleteRefreshCookie() =>
        Response.Cookies.Delete(RefreshCookie, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Path = CookiePath });
}
