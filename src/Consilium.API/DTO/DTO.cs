
namespace Consilium.API.Dtos;

// --- 1. MOVE THE RECORD DEFINITION HERE ---
// A DTO (Data Transfer Object) for creating a client
public record CreateClientRequest(string Email, string Password, string Name, int Phone, int NIF, string Address);
