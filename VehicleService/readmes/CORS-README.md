# CORS - Configuración Implementada

## ¿Qué es CORS?

**CORS (Cross-Origin Resource Sharing)** permite que tu API acepte requests desde otros dominios/puertos.

### Ejemplo del problema que resuelve:

**Sin CORS:**
```
Frontend en http://localhost:3000
Intenta llamar a http://localhost:5000/api/users
❌ Error: "CORS policy: No 'Access-Control-Allow-Origin' header"
```

**Con CORS configurado:**
```
Frontend en http://localhost:3000
Llama a http://localhost:5000/api/users
✅ Funciona correctamente
```

---

## Configuración Implementada

### 1. Archivo de Configuración (appsettings.json)

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",    // React
      "http://localhost:4200",    // Angular
      "http://localhost:5173"     // Vite
    ],
    "AllowAnyOrigin": false,      // Seguridad: solo orígenes específicos
    "AllowCredentials": true      // Permite cookies/auth headers
  }
}
```

### 2. Configuración en Program.cs

```csharp
// Lee la configuración
var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsSettings>();

// Configura la política CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(corsSettings.AllowedOrigins)
              .AllowAnyMethod()      // GET, POST, PUT, DELETE, etc.
              .AllowAnyHeader()      // Content-Type, Authorization, etc.
              .AllowCredentials();   // Cookies, tokens
    });
});

// Aplica CORS en el pipeline
app.UseCors("DefaultCorsPolicy");
```

---

## Opciones de Configuración

### Desarrollo - Permitir cualquier origen (NO usar en producción)

**appsettings.Development.json:**
```json
{
  "Cors": {
    "AllowAnyOrigin": true,
    "AllowCredentials": false
  }
}
```

⚠️ **Nota:** Si usas `AllowAnyOrigin: true`, NO puedes usar `AllowCredentials: true` (restricción de seguridad de los navegadores).

### Producción - Orígenes específicos

**appsettings.json:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://miapp.com",
      "https://www.miapp.com"
    ],
    "AllowAnyOrigin": false,
    "AllowCredentials": true
  }
}
```

---

## Métodos y Headers Permitidos

La configuración actual permite:

**Métodos HTTP:**
- GET
- POST
- PUT
- DELETE
- PATCH
- OPTIONS (preflight automático)

**Headers:**
- Content-Type
- Authorization
- X-Requested-With
- Accept
- Origin
- Cualquier otro header personalizado

**Credentials:**
- Cookies
- Tokens de autenticación
- Headers de autorización

---

## Orden en el Pipeline (Importante)

```csharp
app.UseHttpsRedirection();
app.UseCors("DefaultCorsPolicy");  // ⚠️ CORS debe ir aquí
app.UseAuthorization();
app.MapControllers();
```

**Orden correcto:**
1. UseHttpsRedirection
2. **UseCors** ← Debe estar ANTES de UseAuthorization
3. UseAuthorization
4. MapControllers

---

## Pruebas

### Test desde JavaScript/Fetch

```javascript
// Desde http://localhost:3000
fetch('http://localhost:5000/api/users', {
  method: 'GET',
  credentials: 'include',  // Importante para cookies
  headers: {
    'Content-Type': 'application/json'
  }
})
.then(response => response.json())
.then(data => console.log(data))
.catch(error => console.error('Error:', error));
```

### Test desde axios

```javascript
import axios from 'axios';

axios.defaults.withCredentials = true;

axios.get('http://localhost:5000/api/users')
  .then(response => console.log(response.data))
  .catch(error => console.error(error));
```

### Headers de respuesta esperados

```
Access-Control-Allow-Origin: http://localhost:3000
Access-Control-Allow-Credentials: true
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH, OPTIONS
Access-Control-Allow-Headers: *
```

---

## Agregar Nuevos Orígenes

### Desarrollo local:
Edita **appsettings.Development.json**:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:8080",  // Nuevo
      "http://localhost:5500"   // Nuevo
    ]
  }
}
```

### Producción:
Edita **appsettings.json**:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://miapp.com",
      "https://nuevaapp.com"  // Nuevo
    ]
  }
}
```

**No necesitas recompilar**, solo reinicia la aplicación.

---

## Troubleshooting

### Error: "CORS policy blocked"
✅ Verifica que el origen esté en `AllowedOrigins`
✅ Asegúrate que UseCors está ANTES de UseAuthorization
✅ Reinicia la aplicación después de cambiar appsettings

### Error: "Credentials flag is true, but allowed origins is *"
❌ No puedes usar `AllowAnyOrigin: true` con `AllowCredentials: true`
✅ Usa orígenes específicos O desactiva credentials

### Error: "Method not allowed"
✅ Verifica que `.AllowAnyMethod()` está en la configuración

### Preflight (OPTIONS) falla
✅ El middleware CORS maneja automáticamente OPTIONS
✅ Asegúrate que CORS está configurado correctamente

---

## Seguridad - Mejores Prácticas

### ✅ Hacer:
- Usar orígenes específicos en producción
- Usar HTTPS en producción
- Validar origins dinámicamente si es necesario
- Limitar métodos si no necesitas todos

### ❌ NO Hacer:
- Usar `AllowAnyOrigin: true` en producción
- Permitir cualquier header sin necesidad
- Exponer headers sensibles

### Configuración segura para producción:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionCorsPolicy", policy =>
    {
        policy.WithOrigins("https://miapp.com")
              .WithMethods("GET", "POST", "PUT", "DELETE")  // Solo métodos necesarios
              .WithHeaders("Content-Type", "Authorization") // Solo headers necesarios
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10)); // Cache preflight
    });
});
```

---

## Configuración Avanzada (Opcional)

### Múltiples políticas CORS

```csharp
builder.Services.AddCors(options =>
{
    // Política para admin
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.WithOrigins("https://admin.miapp.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // Política para clientes
    options.AddPolicy("ClientPolicy", policy =>
    {
        policy.WithOrigins("https://app.miapp.com")
              .WithMethods("GET", "POST")
              .AllowAnyHeader();
    });
});

// Aplicar diferentes políticas por controller
[EnableCors("AdminPolicy")]
public class AdminController : ControllerBase { }

[EnableCors("ClientPolicy")]
public class UsersController : ControllerBase { }
```

---

## Resumen

✅ **CORS configurado y listo para usar**
✅ **Configurable desde appsettings.json**
✅ **Seguro por defecto** (orígenes específicos)
✅ **Flexible para desarrollo** (puedes agregar localhost fácilmente)
✅ **Preparado para producción** (solo cambia los orígenes)
