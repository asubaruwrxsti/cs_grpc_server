using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace GrpcServer.Utils
{
    public static class GrpcServiceRegistrar
    {
        public static void RegisterGrpcServices(IServiceCollection services)
        {
            try
            {
                var grpcServices = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(type => type.IsClass &&
                                   !type.IsAbstract &&
                                   type.BaseType != null &&
                                   IsGrpcServiceBase(type.BaseType));

                foreach (var serviceType in grpcServices)
                {
                    services.AddTransient(serviceType);
                    Console.WriteLine($"Registered gRPC service: {serviceType.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during service registration: {ex}");
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