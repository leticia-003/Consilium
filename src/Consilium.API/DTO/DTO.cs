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
    string NIF,
    string? Address,
    // Optional phone info (single/main phone)
    string? PhoneNumber,
    short? PhoneCountryCode,
    bool? PhoneIsMain
);

/// <summary>
/// Request DTO for user login
/// </summary>
public record LoginRequest(
    string Email,
    string Password
);

/// <summary>
/// Request DTO for updating user information
/// </summary>
public record UpdateUserRequest(
    string? Name,
    string? Email,
    string? Password
);

/// <summary>
/// Request DTO for updating client information (updates both User and Client data)
/// </summary>
public record UpdateClientRequest(
    string? Name,
    string? Email,
    string? Password,
    string? Address,
    string? NIF,
    bool? IsActive,
    // Optional phone update
    string? PhoneNumber,
    short? PhoneCountryCode,
    bool? PhoneIsMain
);

/// <summary>
/// Request DTO for creating a new lawyer
/// </summary>
public record CreateLawyerRequest(
    string Email,
    string Password,
    string Name,
    string NIF,
    string ProfessionalRegister,
    // Optional phone info (single/main phone)
    string? PhoneNumber,
    short? PhoneCountryCode,
    bool? PhoneIsMain
);

/// <summary>
/// Request DTO for updating lawyer information (updates both User and Lawyer data)
/// </summary>
public record UpdateLawyerRequest(
    string? Name,
    string? Email,
    string? Password,
    string? ProfessionalRegister,
    // Optional phone update
    string? PhoneNumber,
    short? PhoneCountryCode,
    bool? PhoneIsMain,
    bool? IsActive
);

/// <summary>
/// Request DTO for creating a new admin
/// </summary>
public record CreateAdminRequest(
    string Email,
    string Password,
    string Name,
    string NIF,
    // Optional phone info (single/main phone)
    string? PhoneNumber,
    short? PhoneCountryCode,
    bool? PhoneIsMain
);

/// <summary>
/// Request DTO for updating admin information (updates both User and Admin data)
/// </summary>
public record UpdateAdminRequest(
    string? Name,
    string? Email,
    string? Password,
    // Optional phone update
    string? PhoneNumber,
    short? PhoneCountryCode,
    bool? PhoneIsMain
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
    UserStatus Status
);

/// <summary>
/// Response DTO for Client data (User + Client info)
/// </summary>
public record ClientResponse(
    Guid Id,
    string Email,
    string Name,
    UserStatus Status,
    string NIF,
    string? Address,
    string? Phone,
    short? PhoneCountryCode
);

/// <summary>
/// Response DTO for Lawyer data (User + Lawyer info)
/// </summary>
public record LawyerResponse(
    Guid Id,
    string Email,
    string Name,
    UserStatus Status,
    string NIF,
    string ProfessionalRegister,
    string? Phone,
    short? PhoneCountryCode
);

/// <summary>
/// Response DTO for Admin data (User + Admin info)
/// </summary>
public record AdminResponse(
    Guid Id,
    string Email,
    string Name,
    UserStatus Status,
    string NIF,
    DateTime StartedAt,
    string? Phone,
    short? PhoneCountryCode
);

/// <summary>
/// Response DTO for successful login
/// </summary>
public record LoginResponse(
    string Token,
    Guid UserId,
    string Email,
    string Name,
    UserStatus Status
);
