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
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

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
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

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
}
