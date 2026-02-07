using BookingService.Application.Features.Commands;

namespace BookingService.Application
{
    public static class DependendyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register Handlers
            services.AddScoped<CreateBookingHandler>();

            return services;
        }
    }
}
