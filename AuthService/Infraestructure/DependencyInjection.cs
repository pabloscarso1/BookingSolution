using Microsoft.EntityFrameworkCore;
using AuthService.Application.Features.CreateVehicle;
using AuthService.Application.Features.GetVehicle;
using AuthService.Application.Interfaces;
using AuthService.Infraestructure.Persistence;

namespace AuthService.Infraestructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsql =>
                    {
                        npgsql.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null);
                    }));

            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
