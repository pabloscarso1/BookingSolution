using System.Diagnostics;
using System.Text;

namespace ApiGateway.Api.Middleware;

/// <summary>
/// Middleware centralizado para logging de requests/responses en el API Gateway
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Log request
        await LogRequestAsync(context);

        // Capturar el stream de respuesta original
        var originalBodyStream = context.Response.Body;

        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();

            // Log response
            await LogResponseAsync(context, stopwatch.ElapsedMilliseconds);

            // Copiar la respuesta al stream original
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task LogRequestAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;

        _logger.LogInformation(
            "Incoming Request: {Method} {Path} | Headers: {Headers} | Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}")),
            body);
    }

    private async Task LogResponseAsync(HttpContext context, long elapsedMilliseconds)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        _logger.LogInformation(
            "Outgoing Response: {StatusCode} | Path: {Path} | ElapsedTime: {ElapsedTime}ms | Body: {Body}",
            context.Response.StatusCode,
            context.Request.Path,
            elapsedMilliseconds,
            body.Length > 500 ? body.Substring(0, 500) + "..." : body);
    }
}

/// <summary>
/// Extension para agregar el middleware de logging a la aplicación
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
