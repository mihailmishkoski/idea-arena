using BusinessIdea.Application;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Infrastructure;
using BusinessIdea.Infrastructure.Persistence;
using BusinessIdea.Web.Filters;
using BusinessIdea.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Application composition ------------------------------------------------
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// The current-user abstraction is implemented in the web layer.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Real-time chat + notifications over WebSockets.
builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

// --- Presentation -----------------------------------------------------------
builder.Services.AddControllers(options =>
    options.Filters.Add<ApiExceptionFilterAttribute>());
builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Identity issues auth cookies; point its redirects at our Razor Pages and make
// API calls return 401 instead of redirecting to the login page.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";

    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api") ||
            context.Request.Path.StartsWithSegments("/hubs"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// Apply any pending migrations on startup so the app is runnable out of the box.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// `dotnet run seed` populates a large mock dataset and exits without serving.
if (args.Contains("seed", StringComparer.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<BusinessIdea.Infrastructure.Persistence.DatabaseSeeder>();
    await seeder.SeedAsync();
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// In Development the Angular dev-server proxies to the HTTP endpoint, so an
// HTTPS redirect here turns proxied API calls into 307s. Only redirect in
// non-Development environments.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapHub<BusinessIdea.Web.Hubs.ChatHub>("/hubs/chat");

app.Run();
