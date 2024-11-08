using System.Reflection;

namespace GrpcServer.Utils
{
    public static class GrpcServiceRegistrar
    {
        public static void RegisterGrpcServices(IServiceCollection services)
        {
            Console.WriteLine("Starting gRPC service registration scan...");
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                Console.WriteLine($"Scanning assembly: {assembly.FullName}");

                var grpcServices = assembly.GetTypes()
                    .Where(type => type.IsClass &&
                                  !type.IsAbstract &&
                                  type.BaseType != null &&
                                  IsGrpcServiceBase(type.BaseType))
                    .ToList();

                Console.WriteLine($"Found {grpcServices.Count} gRPC service(s)");

                foreach (var serviceType in grpcServices)
                {
                    Console.WriteLine($"Registering service type: {serviceType.FullName}");
                    Console.WriteLine($"Base type: {serviceType.BaseType?.FullName}");
                    services.AddTransient(serviceType);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine("Error loading types during registration:");
                foreach (var loaderException in ex.LoaderExceptions)
                {
                    Console.WriteLine($"Loader Exception: {loaderException?.Message}");
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during service registration: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public static void MapGrpcServices(IEndpointRouteBuilder app)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var grpcServices = assembly.GetTypes()
                .Where(type => type.IsClass &&
                              !type.IsAbstract &&
                              type.BaseType != null &&
                              IsGrpcServiceBase(type.BaseType))
                .ToList();

            foreach (var serviceType in grpcServices)
            {
                var mapGrpcServiceMethod = typeof(GrpcEndpointRouteBuilderExtensions)
                    .GetMethods()
                    .First(m => m.Name == "MapGrpcService" && m.IsGenericMethod);

                var genericMethod = mapGrpcServiceMethod.MakeGenericMethod(serviceType);
                genericMethod.Invoke(null, new object[] { app });

                Console.WriteLine($"Successfully mapped: {serviceType.Name}");
            }
        }

        private static bool IsGrpcServiceBase(Type type)
        {
            try
            {
                return (type.IsGenericType &&
                        (type.GetGenericTypeDefinition().Name.EndsWith("Base") ||
                         type.GetGenericTypeDefinition().Namespace?.StartsWith("Grpc") == true)) ||
                       (!type.IsGenericType &&
                        (type.Name.EndsWith("Base") ||
                         type.Namespace?.StartsWith("Grpc") == true));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if type is gRPC service base: {ex}");
                return false;
            }
        }
    }
}