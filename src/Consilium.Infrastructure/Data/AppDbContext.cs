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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne<Client>()
                .WithOne(c => c.User)
                .HasForeignKey<Client>(c => c.ID);
        }
    }
}