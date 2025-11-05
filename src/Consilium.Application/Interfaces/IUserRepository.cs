using Consilium.Domain.Models;

namespace Consilium.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetById(Guid id);
        Task<List<User>> GetAll();
        Task<User?> Update(Guid id, User updatedUser);
        // Creating a User is part of creating a Client/Lawyer,
        // so we'll handle it in the Client repository for simplicity.
    }
}