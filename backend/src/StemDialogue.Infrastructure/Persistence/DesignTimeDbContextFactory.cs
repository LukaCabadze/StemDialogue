using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StemDialogue.Infrastructure.Persistence;

/// <summary>
/// Lets <c>dotnet ef migrations add</c> build the SQL Server model without starting the API
/// (no JWT key or database connection required).
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Server=localhost;Database=StemDialogue;Integrated Security=true;TrustServerCertificate=true";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connection, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .Options;
        return new AppDbContext(options);
    }
}
