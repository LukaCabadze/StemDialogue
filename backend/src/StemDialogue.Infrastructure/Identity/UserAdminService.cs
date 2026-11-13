using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Common;
using StemDialogue.Application.Identity;

namespace StemDialogue.Infrastructure.Identity;

/// <summary>CMS account management (Admin only). Every check runs on the server (spec §11).</summary>
public sealed class UserAdminService(UserManager<ApplicationUser> users, AuthService auth, TimeProvider clock) : IUserAdminService
{
    public async Task<IReadOnlyList<UserAdminDto>> ListAsync(CancellationToken ct)
    {
        var all = await users.Users.AsNoTracking().OrderBy(u => u.FullName).ToListAsync(ct);
        var result = new List<UserAdminDto>(all.Count);
        foreach (var user in all) result.Add(await ToDtoAsync(user));
        return result;
    }

    public async Task<UserAdminDto> CreateAsync(UserCreateRequest request, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            EmailConfirmed = true,
            FullName = request.FullName.Trim(),
            IsActive = true,
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
            LockoutEnabled = true,
        };

        var created = await users.CreateAsync(user, request.Password);
        if (!created.Succeeded) throw IdentityErrors.ToValidation(created, nameof(request.Password));

        var role = await users.AddToRoleAsync(user, request.Role);
        if (!role.Succeeded) throw IdentityErrors.ToValidation(role, nameof(request.Role));

        return await ToDtoAsync(user);
    }

    public async Task<UserAdminDto> UpdateAsync(string id, UserUpdateRequest request, string actorId, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(id) ?? throw new NotFoundException("User", id);
        var currentRoles = await users.GetRolesAsync(user);
        var isAdmin = currentRoles.Contains(AppRoles.Admin);
        var losesAdmin = isAdmin && (request.Role != AppRoles.Admin || !request.IsActive);

        if (id == actorId && losesAdmin)
            throw new ForbiddenException("You cannot remove your own administrator access or deactivate yourself.");
        if (losesAdmin && await CountActiveAdminsAsync(ct) <= 1)
            throw new ForbiddenException("At least one active administrator must remain.");

        var deactivated = user.IsActive && !request.IsActive;
        user.FullName = request.FullName.Trim();
        user.IsActive = request.IsActive;
        var updated = await users.UpdateAsync(user);
        if (!updated.Succeeded) throw IdentityErrors.ToValidation(updated, nameof(request.FullName));

        if (!currentRoles.Contains(request.Role) || currentRoles.Count != 1)
        {
            await users.RemoveFromRolesAsync(user, currentRoles);
            await users.AddToRoleAsync(user, request.Role);
        }

        if (deactivated || losesAdmin || !currentRoles.Contains(request.Role))
        {
            // Role or status changes take effect immediately: existing tokens stop working.
            await users.UpdateSecurityStampAsync(user);
            await auth.RevokeAllAsync(user.Id, deactivated ? "deactivated" : "role-changed", clock.GetUtcNow().UtcDateTime, ct);
        }

        return await ToDtoAsync(user);
    }

    public async Task ResetPasswordAsync(string id, PasswordResetRequest request, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(id) ?? throw new NotFoundException("User", id);

        // Validate first so a weak password never leaves the account without one.
        foreach (var validator in users.PasswordValidators)
        {
            var check = await validator.ValidateAsync(users, user, request.NewPassword);
            if (!check.Succeeded) throw IdentityErrors.ToValidation(check, nameof(request.NewPassword));
        }

        await users.RemovePasswordAsync(user);
        var added = await users.AddPasswordAsync(user, request.NewPassword);
        if (!added.Succeeded) throw IdentityErrors.ToValidation(added, nameof(request.NewPassword));

        await users.UpdateSecurityStampAsync(user);
        await auth.RevokeAllAsync(user.Id, "password-reset", clock.GetUtcNow().UtcDateTime, ct);
    }

    public async Task UnlockAsync(string id, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(id) ?? throw new NotFoundException("User", id);
        await users.SetLockoutEndDateAsync(user, null);
        await users.ResetAccessFailedCountAsync(user);
    }

    private async Task<int> CountActiveAdminsAsync(CancellationToken ct)
    {
        var admins = await users.GetUsersInRoleAsync(AppRoles.Admin);
        return admins.Count(a => a.IsActive);
    }

    private async Task<UserAdminDto> ToDtoAsync(ApplicationUser user)
    {
        var roles = await users.GetRolesAsync(user);
        return new UserAdminDto(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            roles.FirstOrDefault() ?? string.Empty,
            user.IsActive,
            user.LockoutEnd is { } end && end > clock.GetUtcNow(),
            user.CreatedAtUtc,
            user.LastLoginAtUtc);
    }
}
