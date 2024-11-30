using System.Reflection;
using FluentValidation;
using Grpc.AspNetCore.Server;
using Kaleido.Common.Services.Grpc.Validation.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Validation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcValidation(this IServiceCollection services, Assembly[]? assemblies = null)
    {
        services.AddSingleton<ValidationInterceptor>();

        if (assemblies == null)
        {
            services.AddValidators();
        }
        else
        {
            services.AddValidators(assemblies);
        }

        services.Configure<GrpcServiceOptions>(options =>
        {
            options.Interceptors.Add<ValidationInterceptor>();
        });

        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null) return services;

        var assemblies = new HashSet<Assembly> { entryAssembly };
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Where(a => a.GetName().Name?.StartsWith("Kaleido.") ?? false);

        foreach (var assembly in loadedAssemblies)
        {
            assemblies.Add(assembly);
        }

        return AddValidators(services, assemblies.ToArray());
    }

    public static IServiceCollection AddValidators(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddValidatorsFromAssemblies(assemblies);
        return services;
    }
}