using Consilium.Domain.Enums;

namespace Consilium.API.Dtos;

// ============================================
// REQUEST DTOs (for incoming data)
// ============================================

/// <summary>
/// Request DTO for creating a new client
/// </summary>
public record CreateClientRequest(
    string Email,
    string Password,
    string Name,
    int Phone,
    int NIF,
    string? Address
);

// ============================================
// RESPONSE DTOs (for outgoing data)
// ============================================

/// <summary>
/// Response DTO for User data
/// </summary>
public record UserResponse(
    Guid Id,
    string Email,
    string Name,
    int? Phone,
    UserStatus Status
);

/// <summary>
/// Response DTO for Client data (User + Client info)
/// </summary>
public record ClientResponse(
    Guid Id,
    string Email,
    string Name,
    int? Phone,
    UserStatus Status,
    int NIF,
    string? Address
);
