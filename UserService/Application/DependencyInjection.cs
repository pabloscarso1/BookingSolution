using FluentValidation;
using UserService.Application.Features.CreateUser;
using UserService.Application.Features.GetUser;
using UserService.Application.Features.Queries;

namespace UserService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
                this IServiceCollection services,
                IConfiguration configuration)
        {
            // Register FluentValidation validators
            services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();

            // Register Handlers
            services.AddScoped<CreateUserHandler>();
            services.AddScoped<GetUserByIdHandler>();
            services.AddScoped<GetUserByNameHandler>();
            services.AddScoped<ValidateCredentialsHandler>();

            return services;
        }
    }
}
