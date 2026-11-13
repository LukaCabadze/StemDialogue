using FluentValidation;

namespace StemDialogue.Application.Identity;

/// <summary>CMS roles. Admin manages users and settings; Editor manages content.</summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Editor = "Editor";

    public static readonly IReadOnlyList<string> All = [Admin, Editor];
}

public static class AppPolicies
{
    /// <summary>Admin or Editor.</summary>
    public const string ContentEditor = "ContentEditor";

    /// <summary>Admin only.</summary>
    public const string Administrator = "Administrator";
}

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthUserDto(string Id, string Email, string FullName, IReadOnlyList<string> Roles);

/// <summary>Result of login/refresh. The refresh token travels only in an HttpOnly cookie.</summary>
public sealed record AuthSession(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    AuthUserDto User);

public sealed record AccessTokenResponse(string AccessToken, DateTime ExpiresAtUtc, AuthUserDto User);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public interface IAuthService
{
    Task<AuthSession> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct);

    /// <summary>Rotates the refresh token. Reuse of a rotated token revokes the whole session family.</summary>
    Task<AuthSession> RefreshAsync(string refreshToken, string? ipAddress, CancellationToken ct);

    /// <summary>Revokes the refresh session and invalidates issued access tokens.</summary>
    Task LogoutAsync(string? refreshToken, string? userId, CancellationToken ct);

    Task<AuthUserDto> MeAsync(string userId, CancellationToken ct);

    Task ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken ct);
}

public sealed record UserCreateRequest(string Email, string FullName, string Role, string Password);

public sealed record UserUpdateRequest(string FullName, string Role, bool IsActive);

public sealed record PasswordResetRequest(string NewPassword);

public sealed record UserAdminDto(
    string Id,
    string Email,
    string FullName,
    string Role,
    bool IsActive,
    bool IsLockedOut,
    DateTime CreatedAtUtc,
    DateTime? LastLoginAtUtc);

public interface IUserAdminService
{
    Task<IReadOnlyList<UserAdminDto>> ListAsync(CancellationToken ct);

    Task<UserAdminDto> CreateAsync(UserCreateRequest request, CancellationToken ct);

    Task<UserAdminDto> UpdateAsync(string id, UserUpdateRequest request, string actorId, CancellationToken ct);

    Task ResetPasswordAsync(string id, PasswordResetRequest request, CancellationToken ct);

    Task UnlockAsync(string id, CancellationToken ct);
}

public static class PasswordRules
{
    public const int MinLength = 12;

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .MinimumLength(MinLength)
            .MaximumLength(128)
            .Must(p => p.Any(char.IsLetter) && p.Any(char.IsDigit))
            .WithMessage($"Use at least {MinLength} characters including letters and digits.");
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().MaximumLength(128);
        RuleFor(x => x.NewPassword).StrongPassword().NotEqual(x => x.CurrentPassword).WithMessage("Choose a new password.");
    }
}

public sealed class UserCreateValidator : AbstractValidator<UserCreateRequest>
{
    public UserCreateValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Role).Must(r => AppRoles.All.Contains(r)).WithMessage($"Role must be one of: {string.Join(", ", AppRoles.All)}.");
        RuleFor(x => x.Password).StrongPassword();
    }
}

public sealed class UserUpdateValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Role).Must(r => AppRoles.All.Contains(r)).WithMessage($"Role must be one of: {string.Join(", ", AppRoles.All)}.");
    }
}

public sealed class PasswordResetValidator : AbstractValidator<PasswordResetRequest>
{
    public PasswordResetValidator() => RuleFor(x => x.NewPassword).StrongPassword();
}
