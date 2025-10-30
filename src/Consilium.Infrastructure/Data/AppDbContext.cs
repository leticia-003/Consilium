using Consilium.Domain.Models;
using Consilium.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Consilium.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tell EF Core about your tables
        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Convert PostgreSQL enum to/from string for EF Core
            var converter = new ValueConverter<UserStatus, string>(
                v => v.ToString(),
                v => (UserStatus)Enum.Parse(typeof(UserStatus), v)
            );

            modelBuilder.Entity<User>()
                .Property(e => e.Status)
                .HasConversion(converter);

            // Define the one-to-one relationship between User and Client
            modelBuilder.Entity<User>()
                .HasOne<Client>()
                .WithOne(c => c.User)
                .HasForeignKey<Client>(c => c.ID);
        }
    }
}