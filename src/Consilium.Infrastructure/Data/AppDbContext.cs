using Consilium.Domain.Models;
using Consilium.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
            // Define the one-to-one relationship between User and Client
            modelBuilder.Entity<User>()
                .HasOne<Client>()
                .WithOne(c => c.User)
                .HasForeignKey<Client>(c => c.ID);
        }
    }
}
