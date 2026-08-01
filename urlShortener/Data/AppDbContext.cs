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
        public DbSet<Click> Clicks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Address>(address =>
            {
                address.Property(a => a.OriginalUrl).HasMaxLength(2048);
                address.Property(a => a.NewUrl).HasMaxLength(400);

                address.HasIndex(a => a.NewUrl).IsUnique();

                // A tela de gerenciamento sempre filtra pelo dono.
                address.HasIndex(a => a.UserId);
            });

            modelBuilder.Entity<Click>(click =>
            {
                click.Property(c => c.RefererHost).HasMaxLength(253);
                click.Property(c => c.DeviceType).HasMaxLength(20);
                click.Property(c => c.Browser).HasMaxLength(40);
                click.Property(c => c.OperatingSystem).HasMaxLength(40);

                // Apagar a URL leva junto o historico de cliques dela.
                click.HasOne(c => c.Address)
                     .WithMany(a => a.Clicks)
                     .HasForeignKey(c => c.AddressId)
                     .OnDelete(DeleteBehavior.Cascade);

                // Os relatorios sempre agrupam por URL e ordenam por data.
                click.HasIndex(c => new { c.AddressId, c.ClickedAt });
            });
        }
    }
}
