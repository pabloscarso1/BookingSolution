using FluentValidation;
using Microsoft.EntityFrameworkCore;
using AuthService.Application.Features.CreateVehicle;
using AuthService.Application.Features.GetVehicle;
using AuthService.Application.Interfaces;
using AuthService.Infraestructure;

namespace AuthService.Application
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
