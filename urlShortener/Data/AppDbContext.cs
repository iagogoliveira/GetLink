using Microsoft.EntityFrameworkCore;
using urlShortener.Models;

namespace urlShortener.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Address> Addresses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Address>()
                .HasIndex(u => u.NewUrl)
                .IsUnique();
        }
    }
}