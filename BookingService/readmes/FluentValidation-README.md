# FluentValidation en UserService

## ¿Qué se ha implementado?

Se ha integrado **FluentValidation** en el proyecto UserService para validar automáticamente las entradas de datos en todos los niveles de la aplicación.

## Componentes implementados

### 1. Validators creados

#### Commands
- **CreateUserCommandValidator** - Valida la creación de usuarios
  - Nombre: requerido, mínimo 3 caracteres, máximo 100 caracteres

#### Queries
- **GetUserByIdQueryValidator** - Valida la consulta por ID
  - ID: requerido y debe ser un GUID válido
  
- **GetUserByNameQueryValidator** - Valida la consulta por nombre
  - Nombre: requerido, mínimo 3 caracteres

#### API Contracts
- **CreateUserRequestValidator** - Valida el request de creación
  - Nombre: requerido, mínimo 3 caracteres, máximo 100 caracteres

### 2. ValidationFilter

Un filtro global que intercepta todas las peticiones y valida automáticamente:
- Los parámetros de entrada en los controladores
- Los requests recibidos
- Retorna respuestas estructuradas con los errores de validación

### 3. ValidatorExtensions

Helper que permite validar manualmente en los handlers con un patrón fluido:
```csharp
return await _validator.ValidateAndExecuteAsync(command, async () =>
{
    // Lógica del handler
});
```

## Cómo usar FluentValidation

### Crear un nuevo validator

1. Crea una clase que herede de `AbstractValidator<T>`:

```csharp
using FluentValidation;

namespace BookingService.Application.UseCases.MiUseCase
{
    public class MiCommandValidator : AbstractValidator<MiCommand>
    {
        public MiCommandValidator()
        {
            RuleFor(x => x.Propiedad)
                .NotEmpty()
                .WithMessage("La propiedad es requerida")
                .MinimumLength(5)
                .WithMessage("Mínimo 5 caracteres");
        }
    }
}
```

2. El validator se registrará automáticamente gracias a:
```csharp
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

### Reglas comunes de validación

```csharp
// Requerido
RuleFor(x => x.Campo).NotEmpty();

// Longitud
RuleFor(x => x.Campo).MinimumLength(3).MaximumLength(100);

// Email
RuleFor(x => x.Email).EmailAddress();

// Expresión regular
RuleFor(x => x.Campo).Matches(@"^[a-zA-Z]+$");

// Rango numérico
RuleFor(x => x.Edad).InclusiveBetween(18, 65);

// Validación personalizada
RuleFor(x => x.Campo).Must(BeValidCustom).WithMessage("Validación personalizada falló");

// Validación condicional
RuleFor(x => x.Campo).NotEmpty().When(x => x.OtroCampo != null);
```

### Respuesta de error

Cuando hay errores de validación, la API retorna:
```json
{
  "message": "Error de validación",
  "errors": {
    "Name": [
      "El nombre es requerido",
      "El nombre debe tener al menos 3 caracteres"
    ]
  }
}
```

## Ventajas de esta implementación

1. **Validación automática**: El ValidationFilter valida automáticamente todas las peticiones
2. **Centralización**: Las reglas de validación están en clases dedicadas
3. **Reutilización**: Los validators se pueden inyectar donde se necesiten
4. **Testeable**: Los validators se pueden probar unitariamente
5. **Mensajes personalizados**: Cada regla puede tener su mensaje específico
6. **Separación de responsabilidades**: La validación está separada de la lógica de negocio

## Ejemplo de uso en un Handler

```csharp
public class MiHandler
{
    private readonly IValidator<MiCommand> _validator;

    public MiHandler(IValidator<MiCommand> validator)
    {
        _validator = validator;
    }

    public async Task<Result<MiDto>> Handle(MiCommand command)
    {
        // Opción 1: Usando el helper
        return await _validator.ValidateAndExecuteAsync(command, async () =>
        {
            // Tu lógica aquí
        });

        // Opción 2: Validación manual
        var validationResult = await _validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result<MiDto>.Failure(errors);
        }
        
        // Tu lógica aquí
    }
}
```

## Testing de validators

```csharp
[Fact]
public void Should_Have_Error_When_Name_Is_Empty()
{
    var validator = new CreateUserCommandValidator();
    var command = new CreateUserCommand("");
    
    var result = validator.TestValidate(command);
    
    result.ShouldHaveValidationErrorFor(x => x.Name);
}
```
