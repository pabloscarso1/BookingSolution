using FluentValidation;
using AuthService.Application.Features.Login;
using AuthService.Application.Interfaces;
using AuthService.Application.ExternalServices;
using AuthService.Application.Services;
using AuthService.Infraestructure.Repositories;

namespace AuthService.Application
{
    public static class DependendyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register Validators
            //services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();

            // Register Handlers
            services.AddScoped<LoginCommandHandler>();
            services.AddScoped<RefreshTokenCommandHandler>();

            // Register Services
            services.AddSingleton<JwtService>();
            services.AddHttpClient<UserServiceClient>();

            // Register Repositories
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            return services;
        }
    }
}
