namespace ApiGateway.Api.Configuration;

/// <summary>
/// Configuración centralizada para CORS que es compartida por todos los servicios
/// </summary>
public class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = [];
    public bool AllowAnyOrigin { get; set; } = false;
    public bool AllowCredentials { get; set; } = false;
}
