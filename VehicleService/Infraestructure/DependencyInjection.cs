using Microsoft.EntityFrameworkCore;
using VehicleService.Application.Features.CreateVehicle;
using VehicleService.Application.Features.GetVehicle;
using VehicleService.Application.Interfaces;
using VehicleService.Infraestructure.Persistence;

namespace VehicleService.Infraestructure
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
