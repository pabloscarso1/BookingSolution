using Microsoft.EntityFrameworkCore;
using BookingService.Application.Domain;

namespace BookingService.Infraestructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<Booking> Reservation => Set<Booking>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
