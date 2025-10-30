using Consilium.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Consilium.Domain.Enums;             // <-- This one for UserStatus
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata; // <-- ADD THIS LINE

namespace Consilium.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .HasOne<Client>()
                .WithOne(c => c.User)
                .HasForeignKey<Client>(c => c.ID);
        }
    }
}