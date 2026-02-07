using Asp.Versioning;
using Common.Api.Configuration;
using Common.Api.Filters;
using Common.Api.HealthChecks;
using Common.Api.Middleware;
using BookingService.Application;
using BookingService.Infraestructure;
using BookingService.Infraestructure.HealthCheck;
using BookingService.Infraestructure.Persistence;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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

_ = builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

    options.TokenValidationParameters = new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
