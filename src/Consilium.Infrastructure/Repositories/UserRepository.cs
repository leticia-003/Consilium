using Consilium.Application.Interfaces;
using Consilium.Domain.Models;
using Consilium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Consilium.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetById(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<List<User>> GetAll()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> Update(Guid id, User updatedUser)
        {
            // Get the existing user
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return null;

            // Update only the provided fields (non-null/non-empty values)
            if (!string.IsNullOrWhiteSpace(updatedUser.Name))
                existingUser.Name = updatedUser.Name;

            if (!string.IsNullOrWhiteSpace(updatedUser.Email))
                existingUser.Email = updatedUser.Email;

            if (!string.IsNullOrWhiteSpace(updatedUser.PasswordHash))
                existingUser.PasswordHash = updatedUser.PasswordHash;

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            return existingUser;
        }
    }
}