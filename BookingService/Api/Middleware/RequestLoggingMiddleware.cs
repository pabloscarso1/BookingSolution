using System.Diagnostics;

namespace BookingService.Api.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.TraceIdentifier;
            var stopwatch = Stopwatch.StartNew();

            // Agregar correlation ID al response header
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.TryAdd("X-Correlation-ID", correlationId);
                return Task.CompletedTask;
            });

            try
            {
                // ✅ Log al inicio del request
                LogRequestStarted(context, correlationId);

                // Ejecutar el siguiente middleware
                await _next(context);

                stopwatch.Stop();

                // ✅ Log al completar exitosamente
                LogRequestCompleted(context, correlationId, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // ✅ Log si falla
                LogRequestFailed(context, correlationId, ex, stopwatch.ElapsedMilliseconds);

                // Re-lanzar la excepción para que el ExceptionHandlingMiddleware la maneje
                throw;
            }
        }

        private void LogRequestStarted(HttpContext context, string correlationId)
        {
            _logger.LogInformation(
                "Request started: {Method} {Path} | TraceId: {TraceId} | CorrelationId: {CorrelationId} | RemoteIP: {RemoteIP}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier,
                correlationId,
                context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );
        }

        private void LogRequestCompleted(HttpContext context, string correlationId, long elapsedMs)
        {
            var statusCode = context.Response.StatusCode;
            var logLevel = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

            _logger.Log(
                logLevel,
                "Request completed: {Method} {Path} | StatusCode: {StatusCode} | ElapsedTime: {ElapsedMs}ms | CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                statusCode,
                elapsedMs,
                correlationId
            );
        }

        private void LogRequestFailed(HttpContext context, string correlationId, Exception ex, long elapsedMs)
        {
            _logger.LogError(
                ex,
                "Unhandled exception: {Method} {Path} | ExceptionType: {ExceptionType} | Message: {Message} | ElapsedTime: {ElapsedMs}ms | CorrelationId: {CorrelationId} | RemoteIP: {RemoteIP} | UserAgent: {UserAgent}",
                context.Request.Method,
                context.Request.Path,
                ex.GetType().Name,
                ex.Message,
                elapsedMs,
                correlationId,
                context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                context.Request.Headers.UserAgent.ToString()
            );
        }
    }
}
