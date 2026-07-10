using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BusinessIdea.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the EF Core tools (e.g. <c>dotnet ef migrations
/// add</c>). It lets migrations be generated without spinning up the web host or
/// a live database. The connection string here is only used to build the model;
/// runtime configuration comes from appsettings.json via the DI registration.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Commands that only build the model (migrations add) work without credentials;
        // ones that connect (database update) need BUSINESSIDEA_CONNECTION set.
        var connectionString = Environment.GetEnvironmentVariable("BUSINESSIDEA_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=businessIdea;Username=postgres";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
