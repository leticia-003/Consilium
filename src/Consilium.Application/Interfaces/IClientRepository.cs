using Consilium.Domain.Models;

namespace Consilium.Application.Interfaces
{
    public interface IClientRepository
    {
        Task<Client?> GetById(Guid id);
        Task<List<Client>> GetAll();
        Task<Client> Create(User user, Client client);
        Task Update(Client client);
        Task Delete(Guid id);
    }
}