using Consilium.Application.Interfaces;
using Consilium.Domain.Models;
using Consilium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Consilium.Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly AppDbContext _context;

        public ClientRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Client?> GetById(Guid id)
        {
            // Use Include to also load the related User data
            return await _context.Clients
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ID == id);
        }

        public async Task<List<Client>> GetAll()
        {
            return await _context.Clients
                .Include(c => c.User)
                .ToListAsync();
        }
        
        public async Task<Client> Create(User user, Client client)
        {
            // This needs to be a transaction
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Set the Client's ID to be the same as the User's ID
                user.ID = Guid.NewGuid(); // Or let Postgres generate it if configured
                client.ID = user.ID;
                // Defensive: ensure Status is one of allowed values (DB has a CHECK constraint)
                // Normalize incoming status to upper-case and fallback to ACTIVE if invalid.
                var status = (user.Status ?? string.Empty).ToUpperInvariant();
                if (status != "ACTIVE" && status != "INACTIVE")
                {
                    status = "ACTIVE";
                }
                user.Status = status;

                // Add the User first
                _context.Users.Add(user);
                
                // Add the Client
                _context.Clients.Add(client);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return client;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log and rethrow with the original exception preserved so logs include DB details
                // (Use Console.WriteLine to ensure the message appears in container logs)
                Console.WriteLine($"Error saving User/Client to DB: {ex.GetType()}: {ex.Message}");
                throw;
            }
        }

        public async Task Update(Client client)
        {
            // Note: This only updates the Client table (e.g., Address)
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            // Deleting the User will cascade and delete the Client
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}