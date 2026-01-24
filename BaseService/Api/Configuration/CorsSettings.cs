namespace BaseService.Api.Configuration
{
    public class CorsSettings
    {
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public bool AllowAnyOrigin { get; set; } = false;
        public bool AllowCredentials { get; set; } = true;
    }
}
