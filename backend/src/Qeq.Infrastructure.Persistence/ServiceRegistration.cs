using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qeq.Application.Interfaces;
using Qeq.Application.Interfaces.Repositories;
using Qeq.Infrastructure.Persistence.Contexts;
using Qeq.Infrastructure.Persistence.Repositories;

namespace Qeq.Infrastructure.Persistence;

public static class ServiceRegistration
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.UseVector()));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IGenericRepositoryAsync<>), typeof(GenericRepositoryAsync<>));
        return services;
    }
}