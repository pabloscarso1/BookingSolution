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
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
