using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Infrastructure.Identity;
using BusinessIdea.Infrastructure.Persistence;
using BusinessIdea.Infrastructure.Persistence.Interceptors;
using BusinessIdea.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BusinessIdea.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer: the PostgreSQL-backed
/// DbContext, the audit interceptor, the <see cref="IApplicationDbContext"/>
/// binding, and ASP.NET Core Identity (which also wires up cookie auth and
/// <c>SignInManager</c>).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = BuildConnectionString(configuration);

        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options.UseNpgsql(connectionString)
                   .AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<DatabaseSeeder>();

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;

                // Registration issues an emailed code; unconfirmed accounts cannot sign in.
                options.SignIn.RequireConfirmedEmail = true;

                // Five failed passwords or verification codes lock the account briefly.
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    /// <summary>
    /// SMTP email for the API process. The Worker owns all outbox email, but the
    /// registration verification code is critical-path: it must go out instantly
    /// and must not depend on the Worker running.
    /// </summary>
    public static IServiceCollection AddAuthEmailServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }

    /// <summary>
    /// Adapters used when processing outbox side effects (email + AI critic).
    /// Registered by the worker on top of <see cref="AddInfrastructureServices"/>.
    /// </summary>
    public static IServiceCollection AddOutboxSideEffectServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));

        services.AddSingleton(new PublicAppUrls
        {
            BaseUrl = configuration["App:PublicBaseUrl"] ?? "http://localhost:4200",
        });

        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IAiCriticUserProvider, AiCriticUserProvider>();
        services.AddHttpClient<IAiCritic, GeminiAiCritic>(client =>
            client.Timeout = TimeSpan.FromSeconds(60));

        return services;
    }

    /// <summary>
    /// Resolves the Npgsql connection string. Managed hosts such as Render supply
    /// a URL-style value (postgres://user:pass@host/db); Npgsql needs key/value
    /// form, so convert it when detected.
    /// </summary>
    private static string BuildConnectionString(IConfiguration configuration)
    {
        var raw = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        if (!raw.StartsWith("postgres://") && !raw.StartsWith("postgresql://"))
        {
            return raw;
        }

        var uri = new Uri(raw);
        var userInfo = uri.UserInfo.Split(':', 2);

        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode = SslMode.Require
        }.ConnectionString;
    }
}
