using GrpcServer.Services;
using GrpcServer.Middlewares;
using GrpcServer.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // Clear any URLs from configuration
            builder.WebHost.UseUrls(); // This clears any URLs from configuration

            // Configure Kestrel
            builder.WebHost.ConfigureKestrel(options =>
            {
                // Clear default endpoints
                options.ConfigureEndpointDefaults(o => o.Protocols =
                    Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);

                // Configure HTTP/2 endpoint
                options.ListenLocalhost(5001, o =>
                {
                    o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                });

                // Optional: Configure HTTP/1.1 endpoint for REST APIs if needed
                options.ListenLocalhost(5002, o =>
                {
                    o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
                });
            });

            // Add services to the container
            builder.Services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaxReceiveMessageSize = 2 * 1024 * 1024; // 2 MB
                options.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB
            });

            builder.Logging.AddConsole();

            // Register all gRPC services dynamically
            GrpcServiceRegistrar.RegisterGrpcServices(builder.Services);

            var app = builder.Build();

            // Configure the HTTP request pipeline
            app.UseRouting();

            app.UseMiddleware<RequestLoggingMiddleware>();
            // app.UseMiddleware<AuthMiddleware>();

            // Map all gRPC services dynamically
            GrpcServiceMapper.MapGrpcServices(app);

            // Map the root endpoint
            app.MapGet("/", () =>
                "Communication with gRPC endpoints must be made through a gRPC client. " +
                "gRPC server is running on port 5001.");

            Console.WriteLine("Server is starting...");
            Console.WriteLine("gRPC endpoint: http://localhost:5001");
            Console.WriteLine("HTTP endpoint: http://localhost:5002");

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error during startup: {ex}");
            throw;
        }
    }
}