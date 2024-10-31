
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            _logger.LogInformation($"Request starting: {context.Request.Method} {context.Request.Path}");
            var sw = System.Diagnostics.Stopwatch.StartNew();

            await _next(context);

            sw.Stop();
            _logger.LogInformation($"Request completed: {context.Request.Method} {context.Request.Path} - Took {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Request failed: {context.Request.Method} {context.Request.Path}");
            throw;
        }
    }
}
