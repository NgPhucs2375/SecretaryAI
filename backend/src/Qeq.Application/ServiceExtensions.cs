using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Qeq.Application.Behaviours;
using System.Reflection;
using Qeq.Application.Interfaces;
using Qeq.Application.Tools;

namespace Qeq.Application;

public static class ServiceExtensions
{
    public static void AddApplicationLayer(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        var toolInterface = typeof(IQeqTool);
        var toolTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && toolInterface.IsAssignableFrom(t));

        foreach (var type in toolTypes)
            services.AddScoped(toolInterface, type);

        services.AddScoped<IToolRegistry, ToolRegistry>();

        services.AddScoped<ToolExecutor>();
        services.AddScoped<Agents.SecretaryAgent>();
    }
}