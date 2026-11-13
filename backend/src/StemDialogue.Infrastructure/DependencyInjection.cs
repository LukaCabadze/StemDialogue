using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Identity;
using StemDialogue.Application.Media;
using StemDialogue.Infrastructure.Html;
using StemDialogue.Infrastructure.Identity;
using StemDialogue.Infrastructure.Persistence;
using StemDialogue.Infrastructure.Persistence.Seed;
using StemDialogue.Infrastructure.Storage;

namespace StemDialogue.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Database:Provider = SqlServer (default, migrations) or Sqlite (local development and tests, schema created on start).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.Section));
        services.Configure<MediaOptions>(configuration.GetSection(MediaOptions.Section));
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.Section))
            .Validate(o => Encoding.UTF8.GetByteCount(o.Key) >= 32 && !o.Key.StartsWith("REPLACE", StringComparison.OrdinalIgnoreCase),
                "Set Jwt__Key to a random secret of at least 32 bytes.")
            .Validate(o => o.AccessTokenMinutes is >= 1 and <= 120, "Jwt__AccessTokenMinutes must be between 1 and 120.")
            .ValidateOnStart();

        var provider = configuration["Database:Provider"] ?? "SqlServer";
        var connection = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Set ConnectionStrings__Default.");

        var useSqlite = provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase);
        if (useSqlite) EnsureSqliteFolder(connection);

        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            if (useSqlite)
                options.UseSqlite(connection);
            else
                options.UseSqlServer(connection, sql => sql.EnableRetryOnFailure(3));

            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services
            .AddIdentityCore<ApplicationUser>(o =>
            {
                o.User.RequireUniqueEmail = true;
                o.Password.RequiredLength = PasswordRules.MinLength;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
                o.Lockout.MaxFailedAccessAttempts = 5;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                o.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<TokenService>();
        services.AddScoped<AuthService>();
        services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AuthService>());
        services.AddScoped<IUserAdminService, UserAdminService>();

        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<IHtmlContentSanitizer, HtmlContentSanitizer>();
        services.AddScoped<DatabaseInitializer>();

        return services;
    }

    /// <summary>SQLite does not create folders; make sure the database file's folder exists.</summary>
    private static void EnsureSqliteFolder(string connection)
    {
        var dataSource = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connection).DataSource;
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:" || dataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase)) return;
        var folder = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
    }
}
