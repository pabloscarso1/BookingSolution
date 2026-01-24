using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VehicleService.Application.Features.CreateVehicle;
using VehicleService.Application.Features.GetVehicle;
using VehicleService.Application.Interfaces;
using VehicleService.Infraestructure;

namespace VehicleService.Application
{
    public static class DependendyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register FluentValidation validators
            //services.AddValidatorsFromAssemblyContaining<CreateVehicleCommandValidator>();

            // Register Handlers
            services.AddScoped<CreateVehicleHandler>();
            services.AddScoped<GetVehicleByIdQueryHandler>();

            return services;
        }
    }
}
