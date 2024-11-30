using System.Reflection;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace Kaleido.Modules.Services.Grpc.Categories.Extensions;

public static class GrpcServiceExtensions
{
    /// <summary>
    /// Maps all gRPC services that inherit from the specified base type.
    /// </summary>
    /// <typeparam name="TBase">The base type of the gRPC services to map (e.g., GrpcCategoriesBase)</typeparam>
    /// <param name="app">The WebApplication instance</param>
    /// <param name="logger">Optional logger for diagnostic information</param>
    /// <param name="assemblies">Optional specific assemblies to scan. If none provided, scans calling assembly</param>
    /// <returns>The WebApplication instance for method chaining</returns>
    public static WebApplication MapGrpcServices<TBase>(
        this WebApplication app,
        ILogger? logger = null,
        params Assembly[] assemblies) where TBase : class
    {
        try
        {
            var assembliesToScan = assemblies.Length > 0
                ? assemblies
                : new[] { Assembly.GetCallingAssembly() };

            logger?.LogInformation("Starting to map gRPC services for base type: {BaseType}", typeof(TBase).Name);

            foreach (var assembly in assembliesToScan)
            {
                logger?.LogDebug("Scanning assembly: {Assembly}", assembly.FullName);

                var grpcServices = assembly.GetTypes()
                    .Where(t => t.IsClass
                        && !t.IsAbstract
                        && t.IsSubclassOf(typeof(TBase)));

                var servicesCount = 0;
                foreach (var serviceType in grpcServices)
                {
                    try
                    {
                        logger?.LogDebug("Attempting to map gRPC service: {ServiceType}", serviceType.Name);

                        var method = typeof(GrpcEndpointRouteBuilderExtensions)
                            .GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService))
                            ?.MakeGenericMethod(serviceType);

                        method?.Invoke(null, new object[] { app });
                        servicesCount++;

                        logger?.LogInformation("Successfully mapped gRPC service: {ServiceType}", serviceType.Name);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "Failed to map gRPC service {ServiceType}", serviceType.Name);
                        throw new InvalidOperationException($"Failed to map gRPC service {serviceType.Name}", ex);
                    }
                }

                logger?.LogInformation("Mapped {Count} gRPC services from assembly {Assembly}",
                    servicesCount, assembly.GetName().Name);
            }

            return app;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to map gRPC services");
            throw new InvalidOperationException("Failed to map gRPC services", ex);
        }
    }
}