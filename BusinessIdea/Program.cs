using BusinessIdea.Application;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Infrastructure;
using BusinessIdea.Infrastructure.Persistence;
using BusinessIdea.Web.Filters;
using BusinessIdea.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Managed hosts (Render, etc.) inject the port to listen on via PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// --- Application composition ------------------------------------------------
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddAuthEmailServices(builder.Configuration);

// The current-user abstraction is implemented in the web layer.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Real-time chat + notifications over WebSockets.
builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

// --- Presentation -----------------------------------------------------------
builder.Services.AddControllers(options =>
    options.Filters.Add<ApiExceptionFilterAttribute>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Identity issues auth cookies; the Angular SPA is the only client, so failed
// auth returns plain status codes instead of redirecting to a login page.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
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

// Serve the compiled Angular app (copied into wwwroot at image build time).
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<BusinessIdea.Web.Hubs.ChatHub>("/hubs/chat");

// Any non-API route falls through to the SPA so client-side routing works.
app.MapFallbackToFile("index.html");

app.Run();
