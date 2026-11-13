using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Identity;
using StemDialogue.Infrastructure.Identity;
using StemDialogue.Infrastructure.Storage;

namespace StemDialogue.Api.Web;

internal static class RateLimits
{
    public const string Auth = "auth";
}

internal static class WebSetup
{
    public static IServiceCollection AddWeb(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<ILanguageContext, RequestLanguageContext>();
        services.AddSingleton<IPublicCacheInvalidator, OutputCacheInvalidator>();
        services.AddScoped<PublicCacheEvictionFilter>();

        services
            .AddControllers(o => o.Filters.Add<ValidationFilter>())
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            });

        services.AddProblemDetails();
        services.AddExceptionHandler<ProblemExceptionHandler>();

        services.AddOutputCache(o =>
        {
            // Public content is identical for every visitor; the URL (including ?lang= and filters) is the key.
            o.AddPolicy(CachePolicies.Public, b => b.Expire(TimeSpan.FromSeconds(60)).Tag(CachePolicies.Tag));
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((o, jwt) =>
            {
                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Value.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Value.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = TokenService.SigningKey(jwt.Value),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = AppClaims.Name,
                    RoleClaimType = AppClaims.Role,
                };
                o.Events = new JwtBearerEvents
                {
                    // Deactivation, role changes, password resets and logout take effect immediately.
                    OnTokenValidated = async context =>
                    {
                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                        var userId = context.Principal?.FindFirst(AppClaims.Subject)?.Value;
                        var user = userId is null ? null : await userManager.FindByIdAsync(userId);
                        if (user is null || !user.IsActive || user.SecurityStamp != context.Principal?.FindFirst(AppClaims.SecurityStamp)?.Value)
                            context.Fail("The session is no longer valid.");
                    },
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AppPolicies.ContentEditor, p => p.RequireRole(AppRoles.Admin, AppRoles.Editor))
            .AddPolicy(AppPolicies.Administrator, p => p.RequireRole(AppRoles.Admin));

        var authPerMinute = configuration.GetValue("RateLimiting:AuthPerMinute", 10);
        var requestsPerMinute = configuration.GetValue("RateLimiting:RequestsPerMinute", 600);
        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.AddPolicy(RateLimits.Auth, http => RateLimitPartition.GetFixedWindowLimiter(
                http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = authPerMinute, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http => RateLimitPartition.GetFixedWindowLimiter(
                http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = requestsPerMinute, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });

        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(o => o.AddDefaultPolicy(p =>
        {
            if (origins.Length > 0) p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }));

        services.Configure<ForwardedHeadersOptions>(o =>
        {
            // The API runs behind the Caddy reverse proxy on a private Docker network.
            o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            o.KnownNetworks.Clear();
            o.KnownProxies.Clear();
        });

        if (environment.IsDevelopment())
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo { Title = "STEM Dialogue API", Version = "v1" });
                o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Access token from POST /api/v1/auth/login",
                });
                o.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = [],
                });
                var xml = Path.Combine(AppContext.BaseDirectory, "StemDialogue.Api.xml");
                if (File.Exists(xml)) o.IncludeXmlComments(xml);
                o.CustomSchemaIds(t => t.FullName?.Replace("StemDialogue.Application.", string.Empty).Replace('+', '.'));
            });
        }

        return services;
    }

    /// <summary>Serves uploaded media under /media with long-lived caching and safe headers.</summary>
    public static IApplicationBuilder UseMediaFiles(this WebApplication app)
    {
        var storage = app.Services.GetRequiredService<IFileStorage>() as LocalFileStorage
            ?? throw new InvalidOperationException("Media serving requires local storage.");
        var basePath = app.Configuration[$"{StorageOptions.Section}:PublicBasePath"] ?? "/media";
        if (!basePath.StartsWith('/')) return app; // served by a CDN

        var types = new FileExtensionContentTypeProvider();
        types.Mappings[".webp"] = "image/webp";
        types.Mappings[".svg"] = "image/svg+xml";

        return app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storage.RootPath),
            RequestPath = basePath.TrimEnd('/'),
            ContentTypeProvider = types,
            OnPrepareResponse = context =>
            {
                var headers = context.Context.Response.Headers;
                headers.CacheControl = "public, max-age=31536000, immutable"; // file names are unique ids
                headers.XContentTypeOptions = "nosniff";
                if (context.File.Name.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                    headers.ContentSecurityPolicy = "default-src 'none'; img-src data:; style-src 'unsafe-inline'; sandbox";
            },
        });
    }

    /// <summary>Baseline security headers; admin and auth responses are never cached.</summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers.XFrameOptions = "DENY";
            var path = context.Request.Path;
            if (path.StartsWithSegments("/api/v1/admin") || path.StartsWithSegments("/api/v1/auth"))
                headers.CacheControl = "no-store";
            else if (path.StartsWithSegments("/api/v1/projects") && HttpMethods.IsGet(context.Request.Method) && context.Response.StatusCode == 200)
                headers.CacheControl = "public, max-age=60";
            return Task.CompletedTask;
        });
        await next();
    });
}
