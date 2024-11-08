using GrpcServer.Utils;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ProgramUtils.ConfigureKestrel(builder);
        ProgramUtils.ConfigureServices(builder.Services);
        ProgramUtils.ConfigureLogging(builder.Logging);

        var app = builder.Build();

        ProgramUtils.ConfigureMiddleware(app);
        ProgramUtils.ConfigureEndpoints(app);

        Console.WriteLine("Server is starting...");
        Console.WriteLine("gRPC endpoint: http://localhost:5001");
        Console.WriteLine("HTTP endpoint: http://localhost:5002");

        await app.RunAsync();
    }
}