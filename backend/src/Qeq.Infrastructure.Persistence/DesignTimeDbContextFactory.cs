using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Qeq.Infrastructure.Persistence.Contexts;

namespace Qeq.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Port=5432;Database=qeq;Username=postgres";

        var password = Environment.GetEnvironmentVariable("PGPASSWORD");
        if (password is not null && !connectionString.Contains("Password", StringComparison.OrdinalIgnoreCase))
            connectionString += $";Password={password}";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.UseVector())
            .Options;
        return new ApplicationDbContext(options);
    }
}