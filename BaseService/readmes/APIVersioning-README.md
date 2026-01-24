# API Versioning

## ¿Qué es API Versioning?

**API Versioning** permite mantener **múltiples versiones** de tu API simultáneamente, permitiendo evolucionar tu API sin romper la compatibilidad con clientes existentes.

---

## Problema que Resuelve

### Sin versionado:
```
❌ Cliente antiguo: GET /api/users → Funciona
✅ Actualizas API con cambios breaking
❌ Cliente antiguo: GET /api/users → Se rompe (respuesta cambiada)
```

### Con versionado:
```
✅ Cliente antiguo: GET /api/v1/users → Sigue funcionando
✅ Cliente nuevo: GET /api/v2/users → Usa nuevas features
✅ Ambos conviven sin problemas
```

---

## Implementación en el Proyecto

### Configuración en Program.cs

```csharp
using Asp.Versioning;

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    
    // Múltiples formas de especificar la versión
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),           // /api/v1/users
        new HeaderApiVersionReader("X-Api-Version"), // Header
        new QueryStringApiVersionReader("api-version") // Query string
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

### Estructura de Controllers

```
Api/Controllers/
├── V1/
│   └── UsersController.cs  (Versión 1.0)
└── V2/
    └── UsersController.cs  (Versión 2.0)
```

---

## 3 Formas de Especificar la Versión

### 1. URL Segment (Recomendado) ✅

```http
GET /api/v1/users
GET /api/v2/users
```

**Controller:**
```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("Version 1.0");
}
```

### 2. Header

```http
GET /api/users
Header: X-Api-Version: 1.0
```

### 3. Query String

```http
GET /api/users?api-version=1.0
```

---

## Ejemplos de Uso

### Versión 1.0 (V1)

```csharp
namespace BaseService.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Respuesta simple
            return Ok(result.Value);
        }
    }
}
```

**Request:**
```http
GET /api/v1/users/123
```

**Response:**
```json
{
  "id": "123",
  "name": "John Doe"
}
```

### Versión 2.0 (V2) - Con mejoras

```csharp
namespace BaseService.Api.Controllers.V2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Respuesta con metadata adicional
            return Ok(new
            {
                data = result.Value,
                metadata = new
                {
                    version = "2.0",
                    timestamp = DateTime.UtcNow
                }
            });
        }
    }
}
```

**Request:**
```http
GET /api/v2/users/123
```

**Response:**
```json
{
  "data": {
    "id": "123",
    "name": "John Doe"
  },
  "metadata": {
    "version": "2.0",
    "timestamp": "2026-01-21T20:30:00Z"
  }
}
```

---

## Headers de Versión

El servidor incluye automáticamente headers informativos:

```http
HTTP/1.1 200 OK
api-supported-versions: 1.0, 2.0
api-deprecated-versions: 
```

---

## Deprecar una Versión

```csharp
[ApiController]
[ApiVersion("1.0", Deprecated = true)]  // Marcada como deprecated
[ApiVersion("2.0")]                      // Versión actual
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    // ...
}
```

**Response headers:**
```http
api-supported-versions: 1.0, 2.0
api-deprecated-versions: 1.0  ← Cliente debe actualizar
```

---

## Versionado de Endpoints Específicos

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    // Disponible en ambas versiones
    [HttpGet("{id}")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    public IActionResult GetById(Guid id) { }

    // Solo disponible en V2
    [HttpGet("search")]
    [MapToApiVersion("2.0")]
    public IActionResult Search(string query) { }
}
```

---

## Casos de Uso Comunes

### Cambio de Formato de Respuesta

**V1:**
```json
{
  "id": "123",
  "name": "John"
}
```

**V2:**
```json
{
  "data": { "id": "123", "name": "John" },
  "metadata": { "version": "2.0" }
}
```

### Agregar Paginación

**V1:** Devuelve todo
```http
GET /api/v1/users
```

**V2:** Con paginación
```http
GET /api/v2/users?page=1&pageSize=10
```

### Cambiar Validaciones

**V1:** Email opcional
```csharp
public class CreateUserRequest
{
    public string Name { get; set; }
    public string? Email { get; set; }  // Opcional
}
```

**V2:** Email requerido
```csharp
public class CreateUserRequestV2
{
    public string Name { get; set; }
    public string Email { get; set; }  // Requerido
}
```

---

## Estrategias de Versionado

### 1. Duplicar Controller (Recomendado)
```
V1/UsersController.cs
V2/UsersController.cs
```
- ✅ Código independiente
- ✅ Fácil mantener
- ❌ Duplicación de código

### 2. Mismo Controller, Múltiples Versiones
```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class UsersController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public IActionResult GetV1() { }

    [HttpGet]
    [MapToApiVersion("2.0")]
    public IActionResult GetV2() { }
}
```
- ✅ Un solo archivo
- ❌ Puede volverse complejo

---

## Versionado en Swagger

Con `AddApiExplorer`, Swagger genera documentación separada por versión:

```
/swagger/v1/swagger.json
/swagger/v2/swagger.json
```

En Swagger UI puedes seleccionar qué versión ver.

---

## Best Practices

### ✅ Hacer:

1. **Usar URL Segment** (`/api/v1/users`)
   - Más claro y visible
   - Funciona con cualquier cliente
   - Fácil de documentar

2. **Versión por mayor** (`v1`, `v2`)
   - No versiones minor (`v1.1`, `v1.2`)
   - Los minor pueden ser breaking

3. **Mantener V1 funcionando**
   - Dar tiempo a clientes para actualizar
   - Deprecar antes de eliminar

4. **Documentar cambios**
   - Release notes claros
   - Fechas de deprecación

5. **Backward compatible cuando posible**
   - Agregar campos opcionales
   - No remover campos existentes (en misma versión)

### ❌ No Hacer:

1. Romper V1 sin previo aviso
2. Tener muchas versiones activas (máx 2-3)
3. Cambiar comportamiento sin cambiar versión
4. Olvidar deprecar versiones antiguas

---

## Plan de Migración

### Fase 1: Lanzar V2
```
✅ V1: Disponible
✅ V2: Disponible (nuevo)
```

### Fase 2: Deprecar V1 (3-6 meses)
```
⚠️ V1: Deprecated (funciona pero con warnings)
✅ V2: Recomendada
```

### Fase 3: Eliminar V1 (6-12 meses)
```
❌ V1: Eliminada
✅ V2: Única versión
```

---

## Testing

### Test de Versión Correcta

```csharp
[Fact]
public async Task V1_Should_Return_Simple_Response()
{
    var response = await _client.GetAsync("/api/v1/users/123");
    
    var json = await response.Content.ReadAsStringAsync();
    Assert.DoesNotContain("metadata", json); // V1 no tiene metadata
}

[Fact]
public async Task V2_Should_Return_With_Metadata()
{
    var response = await _client.GetAsync("/api/v2/users/123");
    
    var json = await response.Content.ReadAsStringAsync();
    Assert.Contains("metadata", json); // V2 incluye metadata
}
```

---

## Monitoreo

### Métricas a seguir:

1. **Uso por versión**
   ```
   V1: 30% de requests
   V2: 70% de requests
   ```

2. **Clientes por versión**
   ```
   V1: 5 clientes
   V2: 15 clientes
   ```

3. **Errores por versión**
   ```
   V1: 0.1% error rate
   V2: 0.05% error rate
   ```

---

## Configuración por Ambiente

### Development (appsettings.Development.json)
```json
{
  "ApiVersioning": {
    "DefaultVersion": "1.0",
    "AssumeDefaultWhenUnspecified": true,
    "AllowMultipleVersions": true
  }
}
```

### Production (appsettings.json)
```json
{
  "ApiVersioning": {
    "DefaultVersion": "2.0",
    "AssumeDefaultWhenUnspecified": false,  // Forzar especificar versión
    "AllowMultipleVersions": false
  }
}
```

---

## Deployment - Elegir Versión al Deployar

### Estrategia 1: Despliegue Gradual (Blue-Green)

**Escenario:** Migrar de V1 a V2 gradualmente

#### Paso 1: Ambas versiones disponibles
```bash
# Deploy con ambas versiones activas
dotnet publish -c Release
```

**Resultado:**
- `/api/v1/users` → Funciona (clientes antiguos)
- `/api/v2/users` → Funciona (clientes nuevos)

**Configuración (appsettings.json):**
```json
{
  "ApiVersioning": {
    "DefaultVersion": "1.0",  // V1 como default
    "AssumeDefaultWhenUnspecified": true
  }
}
```

#### Paso 2: Cambiar default a V2 (después de 1-2 semanas)
```json
{
  "ApiVersioning": {
    "DefaultVersion": "2.0",  // V2 como default
    "AssumeDefaultWhenUnspecified": true
  }
}
```

- Clientes que especifican v1 → Siguen funcionando
- Clientes sin versión → Usan V2

#### Paso 3: Eliminar V1 (después de 3-6 meses)
```bash
# Eliminar carpeta V1
rm -r Api/Controllers/V1/

# Deploy sin V1
dotnet publish -c Release
```

---

### Estrategia 2: Feature Flags para Versiones

**Habilitar/deshabilitar versiones en runtime sin redesplegar**

#### appsettings.json
```json
{
  "FeatureFlags": {
    "EnableV1": true,
    "EnableV2": true,
    "EnableV3": false  // No disponible aún
  }
}
```

#### Middleware de Feature Flag
```csharp
app.Use(async (context, next) =>
{
    var version = context.GetRequestedApiVersion()?.ToString();
    var config = context.RequestServices.GetRequiredService<IConfiguration>();
    
    var isEnabled = version switch
    {
        "1.0" => config.GetValue<bool>("FeatureFlags:EnableV1"),
        "2.0" => config.GetValue<bool>("FeatureFlags:EnableV2"),
        _ => true
    };

    if (!isEnabled)
    {
        context.Response.StatusCode = 410; // Gone
        await context.Response.WriteAsJsonAsync(new
        {
            error = "API_VERSION_DISCONTINUED",
            message = $"API version {version} is no longer available"
        });
        return;
    }

    await next();
});
```

---

### Estrategia 3: Deployment por Ambiente

#### Development
```json
{
  "ApiVersioning": {
    "DefaultVersion": "2.0",
    "AssumeDefaultWhenUnspecified": true
  }
}
```
- Todas las versiones disponibles para testing

#### Staging
```json
{
  "ApiVersioning": {
    "DefaultVersion": "2.0",
    "AssumeDefaultWhenUnspecified": true
  }
}
```
- V1 deprecada pero funcional
- V2 como default

#### Production
```json
{
  "ApiVersioning": {
    "DefaultVersion": "2.0",
    "AssumeDefaultWhenUnspecified": false
  }
}
```
- Solo V2 disponible (V1 eliminada)
- Forzar especificar versión explícitamente

---

### Estrategia 4: Deployment Selectivo por Región

**Escenario:** Diferentes regiones con diferentes versiones

#### Region US (adelantada)
```json
{
  "ApiVersioning": {
    "DefaultVersion": "2.0",
    "EnabledVersions": ["2.0", "3.0"]
  }
}
```

#### Region EU (conservadora)
```json
{
  "ApiVersioning": {
    "DefaultVersion": "1.0",
    "EnabledVersions": ["1.0", "2.0"]
  }
}
```

#### Region LATAM (transición)
```json
{
  "ApiVersioning": {
    "DefaultVersion": "1.5",
    "EnabledVersions": ["1.0", "1.5", "2.0"]
  }
}
```

---

## Deployment con CI/CD

### Azure DevOps Pipeline

```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
      - main
      - release/*

variables:
  - name: apiVersion
    value: '2.0'  # Cambiar según release

stages:
  - stage: Build
    jobs:
      - job: BuildAPI
        steps:
          - task: DotNetCoreCLI@2
            inputs:
              command: 'build'
              projects: '**/*.csproj'
          
          - task: DotNetCoreCLI@2
            inputs:
              command: 'publish'
              publishWebProjects: true
              arguments: '-c Release -o $(Build.ArtifactStagingDirectory)'
          
          - task: PublishBuildArtifacts@1

  - stage: DeployStaging
    jobs:
      - job: Deploy
        steps:
          - task: AzureRmWebAppDeployment@4
            inputs:
              ConnectionType: 'AzureRM'
              WebAppName: 'userservice-staging'
              packageForLinux: '$(Pipeline.Workspace)/**/*.zip'
              AppSettings: '-ApiVersioning:DefaultVersion $(apiVersion)'

  - stage: DeployProduction
    condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
    jobs:
      - job: Deploy
        steps:
          - task: AzureRmWebAppDeployment@4
            inputs:
              WebAppName: 'userservice-prod'
              AppSettings: '-ApiVersioning:DefaultVersion 2.0 -ApiVersioning:AssumeDefaultWhenUnspecified false'
```

### GitHub Actions

```yaml
# .github/workflows/deploy.yml
name: Deploy API

on:
  push:
    branches: [ main, develop ]

env:
  API_VERSION_DEFAULT: '2.0'

jobs:
  deploy-staging:
    if: github.ref == 'refs/heads/develop'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Build
        run: dotnet build -c Release
      
      - name: Publish
        run: dotnet publish -c Release -o ./publish
      
      - name: Set appsettings for Staging
        run: |
          echo '{
            "ApiVersioning": {
              "DefaultVersion": "2.0",
              "AssumeDefaultWhenUnspecified": true,
              "EnabledVersions": ["1.0", "2.0"]
            }
          }' > ./publish/appsettings.Staging.json
      
      - name: Deploy to Staging
        run: |
          # Tu comando de deploy

  deploy-production:
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - name: Set appsettings for Production
        run: |
          echo '{
            "ApiVersioning": {
              "DefaultVersion": "2.0",
              "AssumeDefaultWhenUnspecified": false,
              "EnabledVersions": ["2.0"]
            }
          }' > ./publish/appsettings.Production.json
```

---

## Docker Deployment con Versiones

### Dockerfile con build args

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG API_VERSION=2.0
WORKDIR /src

COPY ["BaseService.csproj", "./"]
RUN dotnet restore

COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ApiVersioning__DefaultVersion=${API_VERSION}
ENV ApiVersioning__AssumeDefaultWhenUnspecified=false

ENTRYPOINT ["dotnet", "BaseService.dll"]
```

### Docker Compose con diferentes versiones

```yaml
version: '3.8'

services:
  userservice-v1:
    build:
      context: .
      args:
        API_VERSION: "1.0"
    ports:
      - "5001:80"
    environment:
      - ApiVersioning__DefaultVersion=1.0
      - ApiVersioning__AssumeDefaultWhenUnspecified=true

  userservice-v2:
    build:
      context: .
      args:
        API_VERSION: "2.0"
    ports:
      - "5002:80"
    environment:
      - ApiVersioning__DefaultVersion=2.0
      - ApiVersioning__AssumeDefaultWhenUnspecified=false
```

### Build y deploy por versión

```bash
# Build V1
docker build --build-arg API_VERSION=1.0 -t userservice:v1 .

# Build V2
docker build --build-arg API_VERSION=2.0 -t userservice:v2 .

# Deploy V1
docker run -d -p 5001:80 -e ApiVersioning__DefaultVersion=1.0 userservice:v1

# Deploy V2
docker run -d -p 5002:80 -e ApiVersioning__DefaultVersion=2.0 userservice:v2
```

---

## Kubernetes Deployment con Versiones

### Deployment separado por versión

```yaml
# deployment-v1.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: userservice-v1
spec:
  replicas: 2
  selector:
    matchLabels:
      app: userservice
      version: v1
  template:
    metadata:
      labels:
        app: userservice
        version: v1
    spec:
      containers:
      - name: userservice
        image: userservice:v1
        ports:
        - containerPort: 80
        env:
        - name: ApiVersioning__DefaultVersion
          value: "1.0"
        - name: ApiVersioning__AssumeDefaultWhenUnspecified
          value: "true"
---
# deployment-v2.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: userservice-v2
spec:
  replicas: 3
  selector:
    matchLabels:
      app: userservice
      version: v2
  template:
    metadata:
      labels:
        app: userservice
        version: v2
    spec:
      containers:
      - name: userservice
        image: userservice:v2
        ports:
        - containerPort: 80
        env:
        - name: ApiVersioning__DefaultVersion
          value: "2.0"
```

### Service con routing por versión

```yaml
apiVersion: v1
kind: Service
metadata:
  name: userservice
spec:
  selector:
    app: userservice
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
---
# Ingress con routing por path
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: userservice-ingress
spec:
  rules:
  - host: api.example.com
    http:
      paths:
      - path: /api/v1
        pathType: Prefix
        backend:
          service:
            name: userservice
            port:
              number: 80
      - path: /api/v2
        pathType: Prefix
        backend:
          service:
            name: userservice
            port:
              number: 80
```

---

## Migración en Producción - Checklist

### Antes del Deploy

- [ ] ✅ Tests de ambas versiones pasando
- [ ] ✅ Documentación actualizada
- [ ] ✅ Clientes notificados de nueva versión
- [ ] ✅ Plan de rollback preparado
- [ ] ✅ Métricas y monitoreo configurados

### Durante el Deploy

- [ ] ✅ Deploy en horario de bajo tráfico
- [ ] ✅ V1 y V2 desplegadas simultáneamente
- [ ] ✅ Health checks pasando
- [ ] ✅ Smoke tests ejecutados
- [ ] ✅ Monitoreo activo

### Después del Deploy

- [ ] ✅ Verificar logs sin errores
- [ ] ✅ Validar métricas (latencia, error rate)
- [ ] ✅ Test manual de endpoints críticos
- [ ] ✅ Notificar a stakeholders

### Periodo de Transición (2-4 semanas)

- [ ] ✅ Monitorear uso de V1 vs V2
- [ ] ✅ Contactar clientes que siguen en V1
- [ ] ✅ Resolver issues reportados
- [ ] ✅ Preparar deprecación de V1

### Deprecación de V1 (después de 3-6 meses)

- [ ] ✅ Anuncio de deprecación (30-60 días antes)
- [ ] ✅ Enviar warning headers en V1
- [ ] ✅ Últimas comunicaciones a clientes
- [ ] ✅ Deploy sin V1
- [ ] ✅ Validar que todo funciona

---

## Variables de Entorno para Deployment

### Configuración dinámica sin recompilar

```bash
# Development
export ApiVersioning__DefaultVersion="1.0"
export ApiVersioning__AssumeDefaultWhenUnspecified="true"

# Staging
export ApiVersioning__DefaultVersion="2.0"
export ApiVersioning__AssumeDefaultWhenUnspecified="true"

# Production
export ApiVersioning__DefaultVersion="2.0"
export ApiVersioning__AssumeDefaultWhenUnspecified="false"
```

### En Azure App Service

```bash
# Azure CLI
az webapp config appsettings set \
  --resource-group myResourceGroup \
  --name myAppService \
  --settings ApiVersioning__DefaultVersion=2.0 \
             ApiVersioning__AssumeDefaultWhenUnspecified=false
```

### En AWS Elastic Beanstalk

```bash
# .ebextensions/environment.config
option_settings:
  - namespace: aws:elasticbeanstalk:application:environment
    option_name: ApiVersioning__DefaultVersion
    value: "2.0"
  - namespace: aws:elasticbeanstalk:application:environment
    option_name: ApiVersioning__AssumeDefaultWhenUnspecified
    value: "false"
```

---

## Rollback Strategy

### Si algo falla en V2

#### Opción 1: Cambiar default a V1
```bash
# Sin redesplegar, solo cambiar configuración
az webapp config appsettings set \
  --name myAppService \
  --settings ApiVersioning__DefaultVersion=1.0
```

#### Opción 2: Deshabilitar V2 con Feature Flag
```json
{
  "FeatureFlags": {
    "EnableV2": false  // Deshabilita V2
  }
}
```

#### Opción 3: Rollback completo
```bash
# Volver a versión anterior del deploy
git revert HEAD
git push
# CI/CD redeploya automáticamente
```

---

## Resumen

✅ **API Versioning configurado**
✅ **V1 y V2 implementadas**
✅ **3 formas de especificar versión** (URL, Header, Query)
✅ **URL Segment como método principal** (`/api/v1/users`)
✅ **Headers informativos automáticos**
✅ **Soporte para deprecación**
✅ **Preparado para evolución sin breaking changes**

**URLs disponibles:**
- `/api/v1/users/{id}` - Versión 1.0
- `/api/v2/users/{id}` - Versión 2.0 (con metadata)
- `/api/v2/users/search` - Endpoint nuevo solo en V2
