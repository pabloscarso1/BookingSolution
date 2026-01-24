using Asp.Versioning;
using BaseService.Api.Configuration;
using BaseService.Api.Filters;
using BaseService.Api.HealthChecks;
using BaseService.Api.Middleware;
using BaseService.Application;
using BaseService.Infraestructure;
using BaseService.Infraestructure.HealthCheck;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using System.IO.Compression;

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
    Log.Information("Iniciando BaseService...");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog
    builder.Host.UseSerilog();

    // Configure CORS
    var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsSettings>() ?? new CorsSettings();
    const string CorsPolicyName = "DefaultCorsPolicy";

    // Add services to the container.

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // Add API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;

        // Métodos de versionado soportados
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),           // /api/v1/bases
            new HeaderApiVersionReader("X-Api-Version"), // Header: X-Api-Version: 1.0
            new QueryStringApiVersionReader("api-version") // ?api-version=1.0
        );
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // Add CORS
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

    // Add Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "db", "sql" })
        .AddCheck<MemoryHealthCheck>("memory", tags: new[] { "memory" });

    // Add Health Checks UI
    builder.Services.AddHealthChecksUI(options =>
    {
        var healthChecksConfig = builder.Configuration.GetSection("HealthChecksUI");
        var evaluationTime = healthChecksConfig.GetValue<int?>("EvaluationTimeInSeconds") ?? 30;
        var maxHistory = healthChecksConfig.GetValue<int?>("MaximumHistoryEntriesPerEndpoint") ?? 50;
        
        options.SetEvaluationTimeInSeconds(evaluationTime);
        options.MaximumHistoryEntriesPerEndpoint(maxHistory);
        
        // Leer endpoints desde configuración
        var healthChecks = healthChecksConfig.GetSection("HealthChecks").Get<HealthCheckConfig[]>();
        if (healthChecks != null)
        {
            foreach (var healthCheck in healthChecks)
            {
                options.AddHealthCheckEndpoint(healthCheck.Name, healthCheck.Uri);
            }
        }
    })
    .AddInMemoryStorage();

    // Add Response Compression
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true; // Habilitar compresión para HTTPS
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();

        // Tipos MIME a comprimir
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
        {
        "application/json",
        "application/xml",
        "text/plain",
        "text/css",
        "text/html",
        "application/javascript",
        "text/json"
        });
    });

    // Configurar niveles de compresión
    builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest; // Fastest, Optimal, SmallestSize
    });

    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });

    var app = builder.Build();

    // Response Compression debe ir primero (antes de otros middlewares)
    app.UseResponseCompression();

    // Configure request logging middleware (debe ir primero para capturar todo)
    app.UseRequestLogging();

    // Configure global exception handling middleware
    app.UseGlobalExceptionHandling();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // CORS debe ir después de UseRouting y antes de UseAuthorization
    app.UseCors(CorsPolicyName);

    app.UseAuthorization();

    app.MapControllers();

    // Health Check Endpoints
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false, // No ejecuta checks, solo verifica que la app responde
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Health Checks UI
    app.MapHealthChecksUI(options =>
    {
        options.UIPath = "/health-ui";
    });

    app.Run();
}
catch (Exception ex)
{

    Log.Fatal(ex, "La aplicación terminó inesperadamente");
}
finally { 
    Log.CloseAndFlush(); 
}
