using Microsoft.AspNetCore.Builder;
using System;
using System.Linq;
using System.Reflection;

namespace GrpcServer.Utils
{
    public static class GrpcServiceMapper
    {
        public static void MapGrpcServices(WebApplication app)
        {
            try
            {
                var grpcServices = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(type =>
                        type.IsClass &&
                        !type.IsAbstract &&
                        type.BaseType != null &&
                        IsGrpcServiceBase(type.BaseType));

                var mapGrpcServiceMethod = typeof(GrpcEndpointRouteBuilderExtensions)
                    .GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService));

                if (mapGrpcServiceMethod == null)
                {
                    throw new InvalidOperationException("MapGrpcService method not found");
                }

                foreach (var serviceType in grpcServices)
                {
                    try
                    {
                        var genericMethod = mapGrpcServiceMethod.MakeGenericMethod(serviceType);
                        genericMethod.Invoke(null, new object[] { app });
                        Console.WriteLine($"Mapped gRPC service: {serviceType.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to map gRPC service {serviceType.Name}: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during service mapping: {ex}");
                throw;
            }
        }

        private static bool IsGrpcServiceBase(Type type)
        {
            try
            {
                return type.IsGenericType &&
                       (type.GetGenericTypeDefinition().Name.EndsWith("Base") ||
                        type.GetGenericTypeDefinition().Namespace?.StartsWith("Grpc") == true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if type is gRPC service base: {ex}");
                return false;
            }
        }
    }
}