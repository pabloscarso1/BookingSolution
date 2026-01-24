using Microsoft.EntityFrameworkCore;
using BaseService.Application.Domain;

namespace BaseService.Infraestructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<Base> Bases => Set<Base>();

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
