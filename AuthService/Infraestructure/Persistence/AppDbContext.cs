using Microsoft.EntityFrameworkCore;
using AuthService.Application.Domain;

namespace AuthService.Infraestructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            
            // Configure RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.Property(e => e.UserId)
                    .IsRequired();
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                
                entity.Property(e => e.ExpiresAt)
                    .IsRequired();
                
                entity.Property(e => e.IsRevoked)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.HasIndex(e => e.Token)
                    .IsUnique();

                entity.HasIndex(e => e.UserId);
            });
        }
    }
}
