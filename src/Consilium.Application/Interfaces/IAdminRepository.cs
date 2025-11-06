using Consilium.Domain.Models;

namespace Consilium.Application.Interfaces
{
    public interface IAdminRepository
    {
        Task<Admin?> GetById(Guid id);
        Task<List<Admin>> GetAll();
        Task<Admin> Create(User user, Admin admin);
        Task<Admin?> UpdateAdminAndUser(Guid adminId, Admin adminUpdates, User userUpdates);
        Task Delete(Guid id);
    }
}
