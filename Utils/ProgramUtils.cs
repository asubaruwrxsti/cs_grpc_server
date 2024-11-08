namespace GrpcServer.Utils
{
    public static class ProgramUtils
    {
        public static void ConfigureKestrel(WebApplicationBuilder builder)
        {
            builder.WebHost.UseUrls();
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureEndpointDefaults(o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);

                options.ListenLocalhost(5001, o =>
                {
                    o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                });

                options.ListenLocalhost(5002, o =>
                {
                    o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
                });
            });
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaxReceiveMessageSize = 2 * 1024 * 1024;
                options.MaxSendMessageSize = 5 * 1024 * 1024;
            });

            GrpcServiceRegistrar.RegisterGrpcServices(services);
        }

        public static void ConfigureLogging(ILoggingBuilder logging)
        {
            logging.AddConsole();
        }

        public static void ConfigureMiddleware(WebApplication app)
        {
            app.UseRouting();
            app.UseMiddleware<RequestLoggingMiddleware>();
            // app.UseMiddleware<AuthMiddleware>();
        }

        public static void ConfigureEndpoints(WebApplication app)
        {
            GrpcServiceRegistrar.MapGrpcServices(app);

            app.MapGet("/", () =>
                "Communication with gRPC endpoints must be made through a gRPC client. " +
                "gRPC server is running on port 5001.");
        }
    }
}