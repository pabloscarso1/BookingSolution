using Microsoft.EntityFrameworkCore;
using VehicleService.Application.Domain;

namespace VehicleService.Infraestructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<Vehicle> Vechiles => Set<Vehicle>();

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
