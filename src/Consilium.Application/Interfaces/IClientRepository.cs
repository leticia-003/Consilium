using Consilium.Domain.Models;

namespace Consilium.Application.Interfaces
{
    public interface IClientRepository
    {
        Task<Client?> GetById(Guid id);
        
        Task<(List<Client> Clients, int TotalCount)> GetAll(
            string? search,
            string? status,
            int page,
            int limit,
            string? sortBy,
            string? sortOrder);

        Task<Client> Create(User user, Client client);
        Task Update(Client client);
        Task Delete(Guid id);
    }

}