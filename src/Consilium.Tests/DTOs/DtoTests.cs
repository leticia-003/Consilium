using Consilium.API.Dtos;
using Consilium.Domain.Enums;

namespace Consilium.Tests.DTOs;

// ============================================
// REQUEST DTOs TESTS
// ============================================

public class CreateClientRequestTests
{
    [Fact]
    public void CreateClientRequest_WithAllRequiredFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new CreateClientRequest(
            Email: "client@test.com",
            Password: "password123",
            Name: "John Doe",
            NIF: "123456789",
            Address: "123 Main St",
            PhoneNumber: "912345678",
            PhoneCountryCode: 351,
            PhoneIsMain: true
        );

        // Assert
        Assert.Equal("client@test.com", dto.Email);
        Assert.Equal("password123", dto.Password);
        Assert.Equal("John Doe", dto.Name);
        Assert.Equal("123456789", dto.NIF);
        Assert.Equal("123 Main St", dto.Address);
        Assert.Equal("912345678", dto.PhoneNumber);
        Assert.Equal((short)351, dto.PhoneCountryCode);
        Assert.True(dto.PhoneIsMain);
    }

    [Fact]
    public void CreateClientRequest_WithNullOptionalFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new CreateClientRequest(
            Email: "client@test.com",
            Password: "password123",
            Name: "John Doe",
            NIF: "123456789",
            Address: null,
            PhoneNumber: null,
            PhoneCountryCode: null,
            PhoneIsMain: null
        );

        // Assert
        Assert.Equal("client@test.com", dto.Email);
        Assert.Null(dto.Address);
        Assert.Null(dto.PhoneNumber);
        Assert.Null(dto.PhoneCountryCode);
        Assert.Null(dto.PhoneIsMain);
    }

    [Fact]
    public void CreateClientRequest_Equality_WorksCorrectly()
    {
        // Arrange
        var dto1 = new CreateClientRequest("test@test.com", "pass", "Name", "NIF", "Address", "Phone", 351, true);
        var dto2 = new CreateClientRequest("test@test.com", "pass", "Name", "NIF", "Address", "Phone", 351, true);
        var dto3 = new CreateClientRequest("different@test.com", "pass", "Name", "NIF", "Address", "Phone", 351, true);

        // Assert
        Assert.Equal(dto1, dto2);
        Assert.NotEqual(dto1, dto3);
    }

    [Fact]
    public void CreateClientRequest_Deconstruction_WorksCorrectly()
    {
        // Arrange
        var dto = new CreateClientRequest("test@test.com", "pass", "Name", "NIF", "Address", "Phone", 351, true);

        // Act
        var (email, password, name, nif, address, phoneNumber, phoneCountryCode, phoneIsMain) = dto;

        // Assert
        Assert.Equal("test@test.com", email);
        Assert.Equal("pass", password);
        Assert.Equal("Name", name);
        Assert.Equal("NIF", nif);
        Assert.Equal("Address", address);
        Assert.Equal("Phone", phoneNumber);
        Assert.Equal((short)351, phoneCountryCode);
        Assert.True(phoneIsMain);
    }
}

public class LoginRequestTests
{
    [Fact]
    public void LoginRequest_WithValidCredentials_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new LoginRequest(
            Email: "user@test.com",
            Password: "securePassword123"
        );

        // Assert
        Assert.Equal("user@test.com", dto.Email);
        Assert.Equal("securePassword123", dto.Password);
    }

    [Fact]
    public void LoginRequest_Equality_WorksCorrectly()
    {
        // Arrange
        var dto1 = new LoginRequest("test@test.com", "password");
        var dto2 = new LoginRequest("test@test.com", "password");
        var dto3 = new LoginRequest("test@test.com", "different");

        // Assert
        Assert.Equal(dto1, dto2);
        Assert.NotEqual(dto1, dto3);
    }

    [Fact]
    public void LoginRequest_Deconstruction_WorksCorrectly()
    {
        // Arrange
        var dto = new LoginRequest("user@test.com", "password123");

        // Act
        var (email, password) = dto;

        // Assert
        Assert.Equal("user@test.com", email);
        Assert.Equal("password123", password);
    }
}

public class UpdateUserRequestTests
{
    [Fact]
    public void UpdateUserRequest_WithAllFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new UpdateUserRequest(
            Name: "Updated Name",
            Email: "updated@test.com",
            Password: "newPassword"
        );

        // Assert
        Assert.Equal("Updated Name", dto.Name);
        Assert.Equal("updated@test.com", dto.Email);
        Assert.Equal("newPassword", dto.Password);
    }

    [Fact]
    public void UpdateUserRequest_WithNullFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new UpdateUserRequest(
            Name: null,
            Email: null,
            Password: null
        );

        // Assert
        Assert.Null(dto.Name);
        Assert.Null(dto.Email);
        Assert.Null(dto.Password);
    }

    [Fact]
    public void UpdateUserRequest_PartialUpdate_WorksCorrectly()
    {
        // Arrange & Act
        var dto = new UpdateUserRequest(
            Name: "New Name",
            Email: null,
            Password: null
        );

        // Assert
        Assert.Equal("New Name", dto.Name);
        Assert.Null(dto.Email);
        Assert.Null(dto.Password);
    }
}

public class UpdateClientRequestTests
{
    [Fact]
    public void UpdateClientRequest_WithAllFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new UpdateClientRequest(
            Name: "Updated Name",
            Email: "updated@test.com",
            Password: "newPassword",
            Address: "New Address",
            NIF: "987654321",
            IsActive: true,
            PhoneNumber: "911111111",
            PhoneCountryCode: 44,
            PhoneIsMain: false
        );

        // Assert
        Assert.Equal("Updated Name", dto.Name);
        Assert.Equal("updated@test.com", dto.Email);
        Assert.Equal("newPassword", dto.Password);
        Assert.Equal("New Address", dto.Address);
        Assert.Equal("987654321", dto.NIF);
        Assert.True(dto.IsActive);
        Assert.Equal("911111111", dto.PhoneNumber);
        Assert.Equal((short)44, dto.PhoneCountryCode);
        Assert.False(dto.PhoneIsMain);
    }

    [Fact]
    public void UpdateClientRequest_WithNullFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new UpdateClientRequest(null, null, null, null, null, null, null, null, null);

        // Assert
        Assert.Null(dto.Name);
        Assert.Null(dto.Email);
        Assert.Null(dto.Password);
        Assert.Null(dto.Address);
        Assert.Null(dto.NIF);
        Assert.Null(dto.IsActive);
        Assert.Null(dto.PhoneNumber);
        Assert.Null(dto.PhoneCountryCode);
        Assert.Null(dto.PhoneIsMain);
    }
}

public class CreateLawyerRequestTests
{
    [Fact]
    public void CreateLawyerRequest_WithAllRequiredFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new CreateLawyerRequest(
            Email: "lawyer@test.com",
            Password: "password123",
            Name: "Jane Doe",
            NIF: "123456789",
            ProfessionalRegister: "OAB123456",
            PhoneNumber: "912345678",
            PhoneCountryCode: 351,
            PhoneIsMain: true
        );

        // Assert
        Assert.Equal("lawyer@test.com", dto.Email);
        Assert.Equal("password123", dto.Password);
        Assert.Equal("Jane Doe", dto.Name);
        Assert.Equal("123456789", dto.NIF);
        Assert.Equal("OAB123456", dto.ProfessionalRegister);
        Assert.Equal("912345678", dto.PhoneNumber);
        Assert.Equal((short)351, dto.PhoneCountryCode);
        Assert.True(dto.PhoneIsMain);
    }

    [Fact]
    public void CreateLawyerRequest_WithNullOptionalFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new CreateLawyerRequest(
            Email: "lawyer@test.com",
            Password: "password123",
            Name: "Jane Doe",
            NIF: "123456789",
            ProfessionalRegister: "OAB123456",
            PhoneNumber: null,
            PhoneCountryCode: null,
            PhoneIsMain: null
        );

        // Assert
        Assert.Null(dto.PhoneNumber);
        Assert.Null(dto.PhoneCountryCode);
        Assert.Null(dto.PhoneIsMain);
    }
}

public class UpdateLawyerRequestTests
{
    [Fact]
    public void UpdateLawyerRequest_WithAllFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new UpdateLawyerRequest(
            Name: "Updated Name",
            Email: "updated@test.com",
            Password: "newPassword",
            ProfessionalRegister: "NEW123",
            NIF: "987654321",
            PhoneNumber: "911111111",
            PhoneCountryCode: 44,
            PhoneIsMain: false,
            IsActive: true
        );

        // Assert
        Assert.Equal("Updated Name", dto.Name);
        Assert.Equal("updated@test.com", dto.Email);
        Assert.Equal("newPassword", dto.Password);
        Assert.Equal("NEW123", dto.ProfessionalRegister);
        Assert.Equal("987654321", dto.NIF);
        Assert.Equal("911111111", dto.PhoneNumber);
        Assert.Equal((short)44, dto.PhoneCountryCode);
        Assert.False(dto.PhoneIsMain);
        Assert.True(dto.IsActive);
    }

    [Fact]
    public void UpdateLawyerRequest_WithNullFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new UpdateLawyerRequest(null, null, null, null, null, null, null, null, null);

        // Assert
        Assert.Null(dto.Name);
        Assert.Null(dto.Email);
        Assert.Null(dto.Password);
        Assert.Null(dto.ProfessionalRegister);
        Assert.Null(dto.NIF);
        Assert.Null(dto.PhoneNumber);
        Assert.Null(dto.PhoneCountryCode);
        Assert.Null(dto.PhoneIsMain);
        Assert.Null(dto.IsActive);
    }
}

public class CreateAdminRequestTests
{
    [Fact]
    public void CreateAdminRequest_WithAllRequiredFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new CreateAdminRequest(
            Email: "admin@test.com",
            Password: "password123",
            Name: "Admin User",
            NIF: "123456789",
            PhoneNumber: "912345678",
            PhoneCountryCode: 351,
            PhoneIsMain: true
        );

        // Assert
        Assert.Equal("admin@test.com", dto.Email);
        Assert.Equal("password123", dto.Password);
        Assert.Equal("Admin User", dto.Name);
        Assert.Equal("123456789", dto.NIF);
        Assert.Equal("912345678", dto.PhoneNumber);
        Assert.Equal((short)351, dto.PhoneCountryCode);
        Assert.True(dto.PhoneIsMain);
    }

    [Fact]
    public void CreateAdminRequest_WithNullOptionalFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new CreateAdminRequest(
            Email: "admin@test.com",
            Password: "password123",
            Name: "Admin User",
            NIF: "123456789",
            PhoneNumber: null,
            PhoneCountryCode: null,
            PhoneIsMain: null
        );

        // Assert
        Assert.Null(dto.PhoneNumber);
        Assert.Null(dto.PhoneCountryCode);
        Assert.Null(dto.PhoneIsMain);
    }
}

public class UpdateAdminRequestTests
{
    [Fact]
    public void UpdateAdminRequest_WithAllFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new UpdateAdminRequest(
            Name: "Updated Name",
            Email: "updated@test.com",
            Password: "newPassword",
            PhoneNumber: "911111111",
            PhoneCountryCode: 44,
            PhoneIsMain: false
        );

        // Assert
        Assert.Equal("Updated Name", dto.Name);
        Assert.Equal("updated@test.com", dto.Email);
        Assert.Equal("newPassword", dto.Password);
        Assert.Equal("911111111", dto.PhoneNumber);
        Assert.Equal((short)44, dto.PhoneCountryCode);
        Assert.False(dto.PhoneIsMain);
    }

    [Fact]
    public void UpdateAdminRequest_WithNullFields_CreatesSuccessfully()
    {
        // Arrange & Act
        var dto = new UpdateAdminRequest(null, null, null, null, null, null);

        // Assert
        Assert.Null(dto.Name);
        Assert.Null(dto.Email);
        Assert.Null(dto.Password);
        Assert.Null(dto.PhoneNumber);
        Assert.Null(dto.PhoneCountryCode);
        Assert.Null(dto.PhoneIsMain);
    }
}

// ============================================
// RESPONSE DTOs TESTS
// ============================================

public class UserResponseTests
{
    [Fact]
    public void UserResponse_WithAllFields_CreatesSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new UserResponse(
            Id: id,
            Email: "user@test.com",
            Name: "User Name",
            Status: UserStatus.ACTIVE
        );

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal("user@test.com", dto.Email);
        Assert.Equal("User Name", dto.Name);
        Assert.Equal(UserStatus.ACTIVE, dto.Status);
    }

    [Fact]
    public void UserResponse_WithInactiveStatus_CreatesSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new UserResponse(
            Id: id,
            Email: "user@test.com",
            Name: "User Name",
            Status: UserStatus.INACTIVE
        );

        // Assert
        Assert.Equal(UserStatus.INACTIVE, dto.Status);
    }

    [Fact]
    public void UserResponse_Equality_WorksCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto1 = new UserResponse(id, "test@test.com", "Name", UserStatus.ACTIVE);
        var dto2 = new UserResponse(id, "test@test.com", "Name", UserStatus.ACTIVE);
        var dto3 = new UserResponse(Guid.NewGuid(), "test@test.com", "Name", UserStatus.ACTIVE);

        // Assert
        Assert.Equal(dto1, dto2);
        Assert.NotEqual(dto1, dto3);
    }

    [Fact]
    public void UserResponse_Deconstruction_WorksCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UserResponse(id, "test@test.com", "Name", UserStatus.ACTIVE);

        // Act
        var (resultId, email, name, status) = dto;

        // Assert
        Assert.Equal(id, resultId);
        Assert.Equal("test@test.com", email);
        Assert.Equal("Name", name);
        Assert.Equal(UserStatus.ACTIVE, status);
    }
}

public class ClientResponseTests
{
    [Fact]
    public void ClientResponse_WithAllFields_CreatesSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new ClientResponse(
            Id: id,
            Email: "client@test.com",
            Name: "Client Name",
            Status: UserStatus.ACTIVE,
            NIF: "123456789",
            Address: "123 Main St",
            Phone: "912345678",
            PhoneCountryCode: 351
        );

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal("client@test.com", dto.Email);
        Assert.Equal("Client Name", dto.Name);
        Assert.Equal(UserStatus.ACTIVE, dto.Status);
        Assert.Equal("123456789", dto.NIF);
        Assert.Equal("123 Main St", dto.Address);
        Assert.Equal("912345678", dto.Phone);
        Assert.Equal((short)351, dto.PhoneCountryCode);
    }

    [Fact]
    public void ClientResponse_WithNullOptionalFields_CreatesSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new ClientResponse(
            Id: id,
            Email: "client@test.com",
            Name: "Client Name",
            Status: UserStatus.ACTIVE,
            NIF: "123456789",
            Address: null,
            Phone: null,
            PhoneCountryCode: null
        );

        // Assert
        Assert.Null(dto.Address);
        Assert.Null(dto.Phone);
        Assert.Null(dto.PhoneCountryCode);
    }
}

public class LawyerResponseTests
{
    [Fact]
    public void LawyerResponse_WithAllFields_CreatesSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new LawyerResponse(
            Id: id,
            Email: "lawyer@test.com",
            Name: "Lawyer Name",
            Status: UserStatus.ACTIVE,
            NIF: "123456789",
            ProfessionalRegister: "OAB123456",
            Phone: "912345678",
            PhoneCountryCode: 351
        );

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal("lawyer@test.com", dto.Email);
        Assert.Equal("Lawyer Name", dto.Name);
        Assert.Equal(UserStatus.ACTIVE, dto.Status);
        Assert.Equal("123456789", dto.NIF);
        Assert.Equal("OAB123456", dto.ProfessionalRegister);
        Assert.Equal("912345678", dto.Phone);
        Assert.Equal((short)351, dto.PhoneCountryCode);
    }

    [Fact]
    public void LawyerResponse_WithNullOptionalFields_CreatesSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new LawyerResponse(
            Id: id,
            Email: "lawyer@test.com",
            Name: "Lawyer Name",
            Status: UserStatus.ACTIVE,
            NIF: "123456789",
            ProfessionalRegister: "OAB123456",
            Phone: null,
            PhoneCountryCode: null
        );

        // Assert
        Assert.Null(dto.Phone);
        Assert.Null(dto.PhoneCountryCode);
    }
}

public class AdminResponseTests
{
    [Fact]
    public void AdminResponse_WithAllFields_CreatesSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var startDate = DateTime.UtcNow;

        // Act
        var dto = new AdminResponse(
            Id: id,
            Email: "admin@test.com",
            Name: "Admin Name",
            Status: UserStatus.ACTIVE,
            NIF: "123456789",
            StartedAt: startDate,
            Phone: "912345678",
            PhoneCountryCode: 351
        );

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal("admin@test.com", dto.Email);
        Assert.Equal("Admin Name", dto.Name);
        Assert.Equal(UserStatus.ACTIVE, dto.Status);
        Assert.Equal("123456789", dto.NIF);
        Assert.Equal(startDate, dto.StartedAt);
        Assert.Equal("912345678", dto.Phone);
        Assert.Equal((short)351, dto.PhoneCountryCode);
    }

    [Fact]
    public void AdminResponse_WithNullOptionalFields_CreatesSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var startDate = DateTime.UtcNow;

        // Act
        var dto = new AdminResponse(
            Id: id,
            Email: "admin@test.com",
            Name: "Admin Name",
            Status: UserStatus.ACTIVE,
            NIF: "123456789",
            StartedAt: startDate,
            Phone: null,
            PhoneCountryCode: null
        );

        // Assert
        Assert.Null(dto.Phone);
        Assert.Null(dto.PhoneCountryCode);
    }
}

public class LoginResponseTests
{
    [Fact]
    public void LoginResponse_WithAllFields_CreatesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var dto = new LoginResponse(
            Token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            UserId: userId,
            Email: "user@test.com",
            Name: "User Name",
            Status: UserStatus.ACTIVE
        );

        // Assert
        Assert.Equal("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...", dto.Token);
        Assert.Equal(userId, dto.UserId);
        Assert.Equal("user@test.com", dto.Email);
        Assert.Equal("User Name", dto.Name);
        Assert.Equal(UserStatus.ACTIVE, dto.Status);
    }

    [Fact]
    public void LoginResponse_Equality_WorksCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto1 = new LoginResponse("token123", userId, "test@test.com", "Name", UserStatus.ACTIVE);
        var dto2 = new LoginResponse("token123", userId, "test@test.com", "Name", UserStatus.ACTIVE);
        var dto3 = new LoginResponse("different", userId, "test@test.com", "Name", UserStatus.ACTIVE);

        // Assert
        Assert.Equal(dto1, dto2);
        Assert.NotEqual(dto1, dto3);
    }

    [Fact]
    public void LoginResponse_Deconstruction_WorksCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new LoginResponse("token123", userId, "test@test.com", "Name", UserStatus.ACTIVE);

        // Act
        var (token, id, email, name, status) = dto;

        // Assert
        Assert.Equal("token123", token);
        Assert.Equal(userId, id);
        Assert.Equal("test@test.com", email);
        Assert.Equal("Name", name);
        Assert.Equal(UserStatus.ACTIVE, status);
    }
}
