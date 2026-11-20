using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qeq.Application.Interfaces;
using Qeq.Domain.Settings;
using Qeq.Infrastructure.Shared.Services;

namespace Qeq.Infrastructure.Shared;

public static class ServiceRegistration
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddTransient<IDateTimeService, DateTimeService>();
        
        services.Configure<DeepSeekSettings>(configuration.GetSection("DeepSeek"));
        services.AddHttpClient<ILlmClient,DeepSeekLlmClient>();
        
        return services;
    }
}