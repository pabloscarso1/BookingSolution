using ApiGateway.Api.Configuration;
using ApiGateway.Api.HealthChecks;
using ApiGateway.Api.Middleware;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.IO.Compression;
using System.Threading.RateLimiting;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Iniciando API Gateway...");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog
    builder.Host.UseSerilog();

    // Cargar configuración CORS
    var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsSettings>() ?? new CorsSettings();
    const string CorsPolicyName = "DefaultCorsPolicy";

    // Cargar configuración Rate Limiting
    var rateLimitingSection = builder.Configuration.GetSection("RateLimiting");
    var permitLimit = rateLimitingSection.GetValue<int?>("PermitLimit") ?? 100;
    var windowSeconds = rateLimitingSection.GetValue<int?>("WindowSeconds") ?? 60;
    var queueLimit = rateLimitingSection.GetValue<int?>("QueueLimit") ?? 0;

    // Add services to the container.
    builder.Services.AddControllers();
    
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    // Add Response Compression
    builder.Services.AddResponseCompression(options =>
    {
        options.Providers.Add<GzipCompressionProvider>();
        options.Providers.Add<BrotliCompressionProvider>();
    });
    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Optimal;
    });
    builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Optimal;
    });

    // Add Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter("default", limiterOptions =>
        {
            limiterOptions.PermitLimit = permitLimit;
            limiterOptions.Window = TimeSpan.FromSeconds(windowSeconds);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = queueLimit;
        });
    });

    // Add Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck<ApiGatewayHealthCheck>("api-gateway-health");

    // Add CORS configuration
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(CorsPolicyName, policy =>
        {
            if (corsSettings.AllowAnyOrigin)
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
            else
            {
                policy.WithOrigins(corsSettings.AllowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader();

                if (corsSettings.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            }
        });
    });

    // Cargar Reverse Proxy desde appsettings.json
    builder.Services
        .AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // Use middleware en orden
    app.UseExceptionHandling();
    app.UseRequestLogging();

    app.UseHttpsRedirection();

    app.UseResponseCompression();

    app.UseCors(CorsPolicyName);

    app.UseRateLimiter();

    app.UseAuthorization();

    app.MapControllers().RequireRateLimiting("default");

    // Map health checks endpoint
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }).DisableRateLimiting();

    // Map Reverse Proxy routes
    app.MapReverseProxy().RequireRateLimiting("default");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "El API Gateway falló durante el inicio");
}
finally
{
    Log.Information("Apagando API Gateway...");
    await Log.CloseAndFlushAsync();
}
