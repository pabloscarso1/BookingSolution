namespace BookingService.Application
{
    public static class DependendyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register Handlers
            //services.AddScoped<GetBaseByIdQueryHandler>();

            return services;
        }
    }
}
