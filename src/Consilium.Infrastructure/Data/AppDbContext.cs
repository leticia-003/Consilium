using Consilium.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Consilium.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Phone> Phones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default schema to core (lowercase)
            modelBuilder.HasDefaultSchema("core");

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasKey(u => u.ID);

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            // Configure Client entity with 1:1 relationship to User
            modelBuilder.Entity<Client>()
                .HasKey(c => c.ID);

            modelBuilder.Entity<Client>()
                .HasOne(c => c.User)
                .WithOne(u => u.Client)
                .HasForeignKey<Client>(c => c.ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Phone entity with 1:N relationship to User
            modelBuilder.Entity<Phone>()
                .HasKey(p => p.ID);

            modelBuilder.Entity<Phone>()
                .HasOne(p => p.User)
                .WithMany(u => u.Phones)
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Phone>()
                .Property(p => p.CountryCode)
                .HasDefaultValue((short)351);
        }
    }
}
