using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(user =>
            {
                // Os tamanhos nao sao so higiene: SQL Server nao indexa
                // nvarchar(max), entao sem eles os indices unicos abaixo falham.
                user.Property(u => u.Name).HasMaxLength(200);
                user.Property(u => u.Login).HasMaxLength(100);
                user.Property(u => u.Email).HasMaxLength(254);
                user.Property(u => u.Password).HasMaxLength(200);

                user.HasIndex(u => u.Login).IsUnique();
                user.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}