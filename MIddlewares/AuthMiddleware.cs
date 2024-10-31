namespace GrpcServer.Middlewares
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthMiddleware> _logger;

        public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                string? authHeader = context.Request.Headers.Authorization;

                if (string.IsNullOrEmpty(authHeader))
                {
                    _logger.LogWarning("Authorization header missing");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                // Add your authentication logic here
                // For example:
                if (!authHeader.StartsWith("Bearer "))
                {
                    _logger.LogWarning("Invalid authorization scheme");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                var token = authHeader.Substring("Bearer ".Length);
                // Validate token here

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }
}