using Consilium.Application.Interfaces;
using Consilium.Domain.Models;
using Consilium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Consilium.Infrastructure.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext _context;

        public AdminRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Admin?> GetById(Guid id)
        {
            return await _context.Admins
                .Include(a => a.User)
                    .ThenInclude(u => u.Phones)
                .FirstOrDefaultAsync(a => a.ID == id);
        }

        public async Task<List<Admin>> GetAll()
        {
            return await _context.Admins
                .Include(a => a.User)
                    .ThenInclude(u => u.Phones)
                .ToListAsync();
        }

        public async Task<Admin> Create(User user, Admin admin)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                user.ID = Guid.NewGuid();
                admin.ID = user.ID;
                
                user.IsActive = true;
                admin.StartedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                _context.Users.Add(user);
                _context.Admins.Add(admin);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Reload with related user and phones
                var loaded = await _context.Admins
                    .Include(a => a.User)
                        .ThenInclude(u => u.Phones)
                    .FirstOrDefaultAsync(a => a.ID == admin.ID);

                return loaded ?? admin;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error saving User/Admin to DB: {ex.GetType()}: {ex.Message}");
                throw;
            }
        }

        public async Task Delete(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"Admin with ID {id} not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<Admin?> UpdateAdminAndUser(Guid adminId, Admin adminUpdates, User userUpdates)
        {
            var existingAdmin = await _context.Admins
                .Include(a => a.User)
                    .ThenInclude(u => u.Phones)
                .FirstOrDefaultAsync(a => a.ID == adminId);

            if (existingAdmin == null)
                return null;

            // Update User fields if provided
            if (!string.IsNullOrWhiteSpace(userUpdates.Name))
                existingAdmin.User.Name = userUpdates.Name;

            if (!string.IsNullOrWhiteSpace(userUpdates.Email))
                existingAdmin.User.Email = userUpdates.Email;

            if (!string.IsNullOrWhiteSpace(userUpdates.PasswordHash))
                existingAdmin.User.PasswordHash = userUpdates.PasswordHash;

            // Note: We don't update StartedAt in the PATCH endpoint - it's immutable after creation

            // Handle phone updates: if the caller provided Phone objects in userUpdates.Phones,
            // we'll treat the first one as the 'main' phone and upsert it.
            if (userUpdates.Phones != null && userUpdates.Phones.Any())
            {
                var phoneUpd = userUpdates.Phones.First();

                // Try to find an existing main phone
                var existingMain = existingAdmin.User.Phones.FirstOrDefault(p => p.IsMain == true);
                if (existingMain != null)
                {
                    // Update existing main phone
                    if (!string.IsNullOrWhiteSpace(phoneUpd.Number))
                        existingMain.Number = phoneUpd.Number;
                    if (phoneUpd.CountryCode != 0)
                        existingMain.CountryCode = phoneUpd.CountryCode;
                    existingMain.IsMain = phoneUpd.IsMain;
                }
                else
                {
                    // Create a new phone record and attach to the user
                    var newPhone = new Phone
                    {
                        ID = Guid.NewGuid(),
                        UserID = existingAdmin.User.ID,
                        Number = phoneUpd.Number ?? string.Empty,
                        CountryCode = phoneUpd.CountryCode != 0 ? phoneUpd.CountryCode : (short)351,
                        IsMain = phoneUpd.IsMain
                    };
                    existingAdmin.User.Phones.Add(newPhone);
                    _context.Phones.Add(newPhone);
                }
            }

            // Only update User and Phone entities, not Admin (to avoid touching StartedAt)
            _context.Users.Update(existingAdmin.User);
            await _context.SaveChangesAsync();

            return existingAdmin;
        }
    }
}
