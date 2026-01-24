using Microsoft.EntityFrameworkCore;
using AuthService.Application.Domain;

namespace AuthService.Infraestructure.Persistence
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
