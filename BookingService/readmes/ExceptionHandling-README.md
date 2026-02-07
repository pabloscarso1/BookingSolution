# Middleware de Manejo de Errores y Excepciones

## ¿Qué se ha implementado?

Se ha implementado un **sistema completo de manejo de errores y excepciones** para la API, incluyendo:

1. **Middleware global de excepciones**
2. **Excepciones personalizadas de dominio**
3. **Respuestas de error estructuradas**
4. **Logging automático de errores**

---

## Componentes implementados

### 1. ExceptionHandlingMiddleware

Middleware global que captura todas las excepciones no controladas y las convierte en respuestas HTTP estructuradas.

**Características:**
- Captura todas las excepciones automáticamente
- Convierte excepciones en respuestas JSON estructuradas
- Registra (log) todas las excepciones
- Oculta detalles sensibles en producción
- Incluye stack trace solo en desarrollo

### 2. Excepciones Personalizadas

#### DomainException (Base)
Excepción base para todas las excepciones de dominio.

```csharp
throw new DomainException("ERROR_CODE", "Mensaje de error");
```

#### NotFoundException
Para recursos no encontrados (HTTP 404).

```csharp
throw new NotFoundException("User", userId);
// o
throw new NotFoundException("El usuario no fue encontrado");
```

#### ConflictException
Para conflictos de negocio (HTTP 409).

```csharp
throw new ConflictException("Ya existe un usuario con ese nombre");
// o
throw new ConflictException("USER_EXISTS", "Usuario duplicado");
```

#### BusinessRuleException
Para violaciones de reglas de negocio (HTTP 422).

```csharp
throw new BusinessRuleException("INSUFFICIENT_BALANCE", "Saldo insuficiente");
```

### 3. ErrorResponse

Estructura estándar para todas las respuestas de error:

```json
{
  "type": "ConflictException",
  "message": "Ya existe un usuario con el nombre 'john'",
  "stackTrace": "...",
  "details": {
    "errorCode": "USER_ALREADY_EXISTS"
  },
  "timestamp": "2026-01-21T10:30:00Z",
  "traceId": "00-abc123-def456-00"
}
```

---

## Mapeo de Excepciones a Status Codes

| Excepción | HTTP Status | Código |
|-----------|-------------|--------|
| `ValidationException` | 400 Bad Request | Errores de validación |
| `ArgumentException` | 400 Bad Request | Argumento inválido |
| `InvalidOperationException` | 400 Bad Request | Operación inválida |
| `DomainException` | 400 Bad Request | Error de dominio |
| `NotFoundException` | 404 Not Found | Recurso no encontrado |
| `KeyNotFoundException` | 404 Not Found | Recurso no encontrado |
| `UnauthorizedAccessException` | 401 Unauthorized | No autorizado |
| `ConflictException` | 409 Conflict | Conflicto |
| `BusinessRuleException` | 422 Unprocessable Entity | Regla de negocio |
| `TimeoutException` | 408 Request Timeout | Tiempo agotado |
| Cualquier otra | 500 Internal Server Error | Error del servidor |

---

## Uso en Handlers

### Opción 1: Result Pattern (actual)

Mantiene el patrón Result para control de flujo:

```csharp
public async Task<Result<UserDto>> Handle(CreateUserCommand command)
{
    var user = await _repository.GetAsync(x => x.Id == command.Id);
    
    if (user is null)
        return Result<UserDto>.Failure("USER_NOT_FOUND");
    
    return Result<UserDto>.Success(new UserDto(user.Id, user.Name));
}
```

### Opción 2: Exception-Based (recomendado con middleware)

Lanza excepciones que el middleware capturará automáticamente:

```csharp
public async Task<UserDto> Handle(CreateUserCommand command)
{
    // Validación automática - lanza ValidationException
    await _validator.ValidateAndThrowAsync(command);
    
    var user = await _repository.GetAsync(x => x.Id == command.Id);
    
    if (user is null)
        throw new NotFoundException("User", command.Id);
    
    return new UserDto(user.Id, user.Name);
}
```

---

## Ejemplos de Uso

### En un Controller

```csharp
[HttpPost]
public async Task<IActionResult> Create(
    [FromBody] CreateUserRequest request,
    [FromServices] CreateUserHandlerWithExceptions handler)
{
    // No necesitas try-catch, el middleware lo maneja
    var result = await handler.Handle(new CreateUserCommand(request.Name));
    return Ok(result);
}
```

### Validación de Negocio

```csharp
public async Task<void> DeleteUser(Guid userId)
{
    var user = await _repository.GetAsync(x => x.Id == userId);
    
    if (user is null)
        throw new NotFoundException("User", userId);
    
    if (user.HasActiveReservations)
        throw new BusinessRuleException(
            "USER_HAS_RESERVATIONS",
            "No se puede eliminar un usuario con reservas activas"
        );
    
    await _repository.DeleteAsync(user);
}
```

### Validación Manual

```csharp
if (string.IsNullOrWhiteSpace(name))
    throw new ArgumentException("El nombre no puede estar vacío", nameof(name));

if (age < 18)
    throw new BusinessRuleException(
        "AGE_RESTRICTION",
        "El usuario debe ser mayor de 18 años"
    );
```

---

## Respuestas de Error - Ejemplos

### Validación (400)

```json
{
  "type": "ValidationException",
  "message": "Error de validación",
  "details": {
    "errors": [
      {
        "property": "Name",
        "message": "El nombre es requerido"
      },
      {
        "property": "Name",
        "message": "El nombre debe tener al menos 3 caracteres"
      }
    ]
  },
  "timestamp": "2026-01-21T10:30:00Z",
  "traceId": "00-abc123-def456-00"
}
```

### No Encontrado (404)

```json
{
  "type": "NotFoundException",
  "message": "User con identificador 'a1b2c3d4-...' no fue encontrado",
  "details": {
    "errorCode": "NOT_FOUND"
  },
  "timestamp": "2026-01-21T10:30:00Z",
  "traceId": "00-abc123-def456-00"
}
```

### Conflicto (409)

```json
{
  "type": "ConflictException",
  "message": "Ya existe un usuario con el nombre 'john'",
  "details": {
    "errorCode": "USER_ALREADY_EXISTS"
  },
  "timestamp": "2026-01-21T10:30:00Z",
  "traceId": "00-abc123-def456-00"
}
```

### Error Interno (500)

**En Desarrollo:**
```json
{
  "type": "NullReferenceException",
  "message": "Object reference not set to an instance of an object",
  "stackTrace": "at BookingService...",
  "details": {
    "innerException": {
      "type": "SqlException",
      "message": "Connection timeout",
      "stackTrace": "..."
    }
  },
  "timestamp": "2026-01-21T10:30:00Z",
  "traceId": "00-abc123-def456-00"
}
```

**En Producción:**
```json
{
  "type": "Exception",
  "message": "Ocurrió un error interno en el servidor",
  "timestamp": "2026-01-21T10:30:00Z",
  "traceId": "00-abc123-def456-00"
}
```

---

## Ventajas del Sistema

1. **Centralización**: Todo el manejo de errores en un solo lugar
2. **Consistencia**: Todas las respuestas de error tienen el mismo formato
3. **Logging automático**: Todas las excepciones se registran
4. **Seguridad**: No expone información sensible en producción
5. **Debugging**: Stack traces completos en desarrollo
6. **TraceId**: Facilita el seguimiento de errores
7. **Extensibilidad**: Fácil agregar nuevos tipos de excepciones

---

## Configuración en Program.cs

```csharp
var app = builder.Build();

// IMPORTANTE: El middleware debe estar ANTES de UseRouting/UseEndpoints
app.UseMiddleware<ExceptionHandlingMiddleware>();
// o usando la extensión:
// app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
```

---

## Testing

### Test de Excepciones

```csharp
[Fact]
public async Task Handle_Should_ThrowNotFoundException_When_UserDoesNotExist()
{
    // Arrange
    var handler = new GetUserHandler(_mockRepository.Object);
    
    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(
        () => handler.Handle(new GetUserQuery(Guid.NewGuid()))
    );
}
```

### Test de Middleware

```csharp
[Fact]
public async Task Middleware_Should_Return404_When_NotFoundExceptionThrown()
{
    // Arrange
    var middleware = new ExceptionHandlingMiddleware(
        next: (context) => throw new NotFoundException("User", id),
        logger: _mockLogger.Object,
        environment: _mockEnvironment.Object
    );
    
    // Act
    await middleware.InvokeAsync(_httpContext);
    
    // Assert
    Assert.Equal(404, _httpContext.Response.StatusCode);
}
```

---

## Recomendaciones

1. **Usa excepciones personalizadas** para errores predecibles del dominio
2. **Usa Result pattern** si prefieres control de flujo sin excepciones
3. **No captures excepciones** en los controllers - deja que el middleware lo haga
4. **Incluye códigos de error** para facilitar la integración con clientes
5. **Documenta los códigos de error** en tu API documentation
6. **Usa logging estructurado** con información contextual
