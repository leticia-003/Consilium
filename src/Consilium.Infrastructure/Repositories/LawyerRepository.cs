using Consilium.Application.Interfaces;
using Consilium.Domain.Models;
using Consilium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Consilium.Infrastructure.Repositories
{
    public class LawyerRepository : ILawyerRepository
    {
        private readonly AppDbContext _context;

        public LawyerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Lawyer?> GetById(Guid id)
        {
            // Use Include to also load the related User data
            return await _context.Lawyers
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.ID == id);
        }

        public async Task<(List<Lawyer> Lawyers, int TotalCount)> GetAll(
            string? search,
            string? status,
            int page,
            int limit,
            string? sortBy,
            string? sortOrder)
        {
            var query = _context.Lawyers
                .Include(l => l.User)
                .AsQueryable();

            // Text search across Name, Email, NIF, and Professional Register
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l =>
                    l.User.Name.Contains(search) ||
                    l.User.Email.Contains(search) ||
                    l.User.NIF.Contains(search) ||
                    l.ProfessionalRegister.Contains(search));
            }

            // Filter by status (IsActive boolean)
            if (!string.IsNullOrWhiteSpace(status))
            {
                var isActive = status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase);
                query = query.Where(l => l.User.IsActive == isActive);
            }

            // Count before pagination
            var totalCount = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                sortBy = sortBy.ToLower();
                sortOrder = sortOrder?.ToLower() ?? "asc";

                if (sortBy == "nif")
                    query = sortOrder == "desc" ? query.OrderByDescending(l => l.User.NIF) : query.OrderBy(l => l.User.NIF);
                else if (sortBy == "register")
                    query = sortOrder == "desc" ? query.OrderByDescending(l => l.ProfessionalRegister) : query.OrderBy(l => l.ProfessionalRegister);
                else
                    query = sortOrder == "desc" ? query.OrderByDescending(l => l.User.Name) : query.OrderBy(l => l.User.Name);
            }

            // Pagination
            var lawyers = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (lawyers, totalCount);
        }

        public async Task<Lawyer> Create(User user, Lawyer lawyer)
        {
            // This needs to be a transaction
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Set the Lawyer's ID to be the same as the User's ID
                user.ID = Guid.NewGuid();
                lawyer.ID = user.ID;
                
                // Ensure IsActive is set (should default to true)
                user.IsActive = true;

                // Add the User first
                _context.Users.Add(user);
                
                // Add the Lawyer
                _context.Lawyers.Add(lawyer);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return lawyer;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error saving User/Lawyer to DB: {ex.GetType()}: {ex.Message}");
                throw;
            }
        }

        public async Task Update(Lawyer lawyer)
        {
            // Note: This only updates the Lawyer table (e.g., ProfessionalRegister)
            _context.Lawyers.Update(lawyer);
            await _context.SaveChangesAsync();
        }

        public async Task<Lawyer?> UpdateLawyerAndUser(Guid lawyerId, Lawyer lawyerUpdates, User userUpdates)
        {
            // Get the existing lawyer with its user
            var existingLawyer = await _context.Lawyers
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.ID == lawyerId);

            if (existingLawyer == null)
                return null;

            // Update User fields if provided
            if (!string.IsNullOrWhiteSpace(userUpdates.Name))
                existingLawyer.User.Name = userUpdates.Name;

            if (!string.IsNullOrWhiteSpace(userUpdates.Email))
                existingLawyer.User.Email = userUpdates.Email;

            if (!string.IsNullOrWhiteSpace(userUpdates.PasswordHash))
                existingLawyer.User.PasswordHash = userUpdates.PasswordHash;

            // Update Lawyer fields if provided
            if (!string.IsNullOrWhiteSpace(lawyerUpdates.ProfessionalRegister))
                existingLawyer.ProfessionalRegister = lawyerUpdates.ProfessionalRegister;

            _context.Lawyers.Update(existingLawyer);
            _context.Users.Update(existingLawyer.User);
            await _context.SaveChangesAsync();

            return existingLawyer;
        }

        public async Task Delete(Guid id)
        {
            // Get the user and lawyer
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"Lawyer with ID {id} not found");

            // TODO: Check if lawyer has active/open cases when PROCESS table is implemented
            // For now, we just delete the user (which will cascade delete the lawyer due to 1:1 relationship)
            
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
