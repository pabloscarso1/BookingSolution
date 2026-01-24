# Middleware de Logging de Requests

## Implementación

Se ha agregado un middleware de logging completo que registra todos los requests HTTP en las siguientes instancias:

### ✅ Al inicio del request
```
Request started: GET /api/users/123 | TraceId: 0HMVR... | CorrelationId: 0HMVR... | RemoteIP: 127.0.0.1
```

**Información capturada:**
- Método HTTP (GET, POST, etc.)
- Path completo
- TraceId (generado por ASP.NET Core)
- CorrelationId (mismo que TraceId)
- IP remota del cliente

### ✅ Al completar exitosamente
```
Request completed: GET /api/users/123 | StatusCode: 200 | ElapsedTime: 45ms | CorrelationId: 0HMVR...
```

**Información capturada:**
- StatusCode HTTP
- Tiempo de ejecución en milisegundos
- CorrelationId para correlacionar con el inicio

### ✅ Si ocurre una excepción
```
Unhandled exception: GET /api/users/123 | ExceptionType: NotFoundException | Message: User con identificador '123' no fue encontrado | ElapsedTime: 23ms | CorrelationId: 0HMVR... | RemoteIP: 127.0.0.1 | UserAgent: Mozilla/5.0...
```

**Información capturada:**
- Tipo de excepción
- Mensaje de error
- Stack trace completo (automático con LogError)
- Tiempo hasta la excepción
- IP y UserAgent del cliente

---

## Orden de Middlewares

**IMPORTANTE:** El orden de los middlewares es crucial:

```csharp
// 1. RequestLogging - debe ir primero para capturar TODO
app.UseRequestLogging();

// 2. ExceptionHandling - captura excepciones después del logging
app.UseGlobalExceptionHandling();

// 3. Resto de middlewares
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
```

### ¿Por qué este orden?

1. **RequestLogging primero**: Captura el inicio de TODOS los requests, incluso los que fallan
2. **ExceptionHandling después**: Maneja las excepciones y las convierte en respuestas HTTP
3. El RequestLogging captura la excepción antes que ExceptionHandling la maneje

---

## Características Adicionales

### Header X-Correlation-ID

El middleware agrega automáticamente un header `X-Correlation-ID` en todas las respuestas:

```http
HTTP/1.1 200 OK
X-Correlation-ID: 0HMVR8Q2K3L4M5N6O7P8Q9R0
Content-Type: application/json
```

Esto permite:
- Rastrear un request específico a través de todos los logs
- Debugging más fácil
- Correlacionar logs entre microservicios

### Niveles de Log Automáticos

El middleware ajusta automáticamente el nivel de log según el resultado:

- **StatusCode < 400**: `Information` (logs normales)
- **StatusCode >= 400**: `Warning` (errores del cliente)
- **Excepciones**: `Error` (errores del servidor)

---

## Ejemplos de Output

### Request Exitoso

```
[2026-01-21 10:30:00.123] [Information] Request started: GET /api/users/a1b2c3d4-... | TraceId: 0HMVR8Q2K3L4 | CorrelationId: 0HMVR8Q2K3L4 | RemoteIP: 192.168.1.100
[2026-01-21 10:30:00.168] [Information] Request completed: GET /api/users/a1b2c3d4-... | StatusCode: 200 | ElapsedTime: 45ms | CorrelationId: 0HMVR8Q2K3L4
```

### Request con Error 404

```
[2026-01-21 10:31:00.123] [Information] Request started: GET /api/users/invalid | TraceId: 0HMVR8Q2K3L5 | CorrelationId: 0HMVR8Q2K3L5 | RemoteIP: 192.168.1.100
[2026-01-21 10:31:00.145] [Warning] Request completed: GET /api/users/invalid | StatusCode: 404 | ElapsedTime: 22ms | CorrelationId: 0HMVR8Q2K3L5
```

### Request con Excepción No Controlada

```
[2026-01-21 10:32:00.123] [Information] Request started: POST /api/users | TraceId: 0HMVR8Q2K3L6 | CorrelationId: 0HMVR8Q2K3L6 | RemoteIP: 192.168.1.100
[2026-01-21 10:32:00.156] [Error] Unhandled exception: POST /api/users | ExceptionType: NullReferenceException | Message: Object reference not set to an instance of an object | ElapsedTime: 33ms | CorrelationId: 0HMVR8Q2K3L6 | RemoteIP: 192.168.1.100 | UserAgent: PostmanRuntime/7.26.8
   at BaseService.Application.UseCases.CreateUserHandler.Handle(CreateUserCommand command) in CreateUserCommand.cs:line 35
   at BaseService.Api.Controllers.UsersController.Create(CreateUserRequest request, CreateUserHandler handler) in UsersController.cs:line 20
   ... [stack trace completo]
[2026-01-21 10:32:00.189] [Information] Ocurrió una excepción no controlada: Object reference not set...
```

---

## Configuración en appsettings.json

### Desarrollo (appsettings.Development.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "BaseService.Api.Middleware.RequestLoggingMiddleware": "Information",
      "BaseService.Api.Middleware.ExceptionHandlingMiddleware": "Error"
    }
  }
}
```

### Producción (appsettings.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "BaseService.Api.Middleware.RequestLoggingMiddleware": "Information",
      "BaseService.Api.Middleware.ExceptionHandlingMiddleware": "Error"
    }
  }
}
```

---

## Integración con Proveedores de Logging

### Serilog (Recomendado para producción)

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

```csharp
// Program.cs
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

### Azure Application Insights

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

---

## Ventajas del Sistema

1. **Trazabilidad completa**: Cada request tiene un CorrelationId único
2. **Performance monitoring**: Tiempo de ejecución de cada request
3. **Debugging facilitado**: Stack traces completos con contexto
4. **Auditoría**: Registro de IPs, métodos, paths
5. **Integración lista**: Compatible con cualquier proveedor de logging
6. **Producción ready**: Niveles de log apropiados por defecto

---

## Filtrado de Logs Sensibles

Si necesitas excluir ciertos paths del logging (e.g., health checks):

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // Excluir health checks del logging detallado
    if (context.Request.Path.StartsWithSegments("/health"))
    {
        await _next(context);
        return;
    }

    // ... resto del código
}
```

---

## Búsqueda de Logs por CorrelationId

Ejemplo de búsqueda en logs:

```bash
# Buscar todos los logs de un request específico
grep "0HMVR8Q2K3L4" logs/log-20260121.txt

# Buscar solo errores
grep "Error.*CorrelationId" logs/log-20260121.txt

# Buscar requests lentos (>1000ms)
grep "ElapsedTime: [0-9]\{4,\}ms" logs/log-20260121.txt
```
