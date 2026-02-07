using System.Net;
using System.Text.Json;
using FluentValidation;
using BookingService.Application.Exceptions;

namespace BookingService.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log la excepción
            _logger.LogError(exception, "Ocurrió una excepción no controlada: {Message}", exception.Message);

            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                TraceId = context.TraceIdentifier,
                Type = exception.GetType().Name
            };

            // Determinar el tipo de excepción y configurar la respuesta
            switch (exception)
            {
                case ValidationException validationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = "Error de validación";
                    errorResponse.Details = new Dictionary<string, object>
                    {
                        ["errors"] = validationException.Errors.Select(e => new
                        {
                            property = e.PropertyName,
                            message = e.ErrorMessage
                        })
                    };
                    break;

                case NotFoundException notFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Message = notFoundException.Message;
                    errorResponse.Details = new Dictionary<string, object>
                    {
                        ["errorCode"] = notFoundException.ErrorCode
                    };
                    break;

                case ConflictException conflictException:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.Message = conflictException.Message;
                    errorResponse.Details = new Dictionary<string, object>
                    {
                        ["errorCode"] = conflictException.ErrorCode
                    };
                    break;

                case BusinessRuleException businessRuleException:
                    response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    errorResponse.Message = businessRuleException.Message;
                    errorResponse.Details = new Dictionary<string, object>
                    {
                        ["errorCode"] = businessRuleException.ErrorCode
                    };
                    break;

                case DomainException domainException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = domainException.Message;
                    errorResponse.Details = new Dictionary<string, object>
                    {
                        ["errorCode"] = domainException.ErrorCode
                    };
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Message = "Recurso no encontrado";
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Message = "No autorizado";
                    break;

                case InvalidOperationException invalidOpException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = invalidOpException.Message;
                    break;

                case ArgumentException argumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = argumentException.Message;
                    break;

                case TimeoutException:
                    response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    errorResponse.Message = "La solicitud tardó demasiado tiempo";
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = _environment.IsDevelopment()
                        ? exception.Message
                        : "Ocurrió un error interno en el servidor";
                    break;
            }

            // Solo incluir stack trace en desarrollo
            if (_environment.IsDevelopment())
            {
                errorResponse.StackTrace = exception.StackTrace;
                
                if (exception.InnerException != null)
                {
                    errorResponse.Details ??= new Dictionary<string, object>();
                    errorResponse.Details["innerException"] = new
                    {
                        type = exception.InnerException.GetType().Name,
                        message = exception.InnerException.Message,
                        stackTrace = exception.InnerException.StackTrace
                    };
                }
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var result = JsonSerializer.Serialize(errorResponse, jsonOptions);
            await response.WriteAsync(result);
        }
    }
}
