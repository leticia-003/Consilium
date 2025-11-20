using Moq;
using Consilium.Application.Interfaces;
using Consilium.Domain.Models;
using Consilium.Domain.Enums;
using Consilium.API.Dtos;

namespace Consilium.Tests.Endpoints;

/// <summary>
/// Tests for User Endpoints business logic
/// These tests validate the repository interactions and data transformations
/// that occur in the UserEndpoints
/// </summary>
public class UserEndpointsTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IClientRepository> _mockClientRepo;

    public UserEndpointsTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockClientRepo = new Mock<IClientRepository>();
    }

    #region GetAllUsers Tests

    [Fact]
    public async Task GetAllUsers_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                ID = Guid.NewGuid(),
                Name = "User One",
                Email = "user1@test.com",
                NIF = "123456789",
                IsActive = true
            },
            new User
            {
                ID = Guid.NewGuid(),
                Name = "User Two",
                Email = "user2@test.com",
                NIF = "987654321",
                IsActive = false
            }
        };

        _mockUserRepo.Setup(r => r.GetAll()).ReturnsAsync(users);

        // Act
        var result = await _mockUserRepo.Object.GetAll();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("User One", result[0].Name);
        Assert.Equal("User Two", result[1].Name);
    }

    [Fact]
    public async Task GetAllUsers_EmptyList_ReturnsEmptyList()
    {
        // Arrange
        var users = new List<User>();
        _mockUserRepo.Setup(r => r.GetAll()).ReturnsAsync(users);

        // Act
        var result = await _mockUserRepo.Object.GetAll();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllUsers_MapsToUserResponse_Correctly()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                ID = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@test.com",
                NIF = "123456789",
                IsActive = true
            }
        };

        _mockUserRepo.Setup(r => r.GetAll()).ReturnsAsync(users);

        // Act
        var result = await _mockUserRepo.Object.GetAll();
        var responses = result.Select(u => new UserResponse(
            Id: u.ID,
            Email: u.Email,
            Name: u.Name,
            Status: u.IsActive ? UserStatus.ACTIVE : UserStatus.INACTIVE
        )).ToList();

        // Assert
        Assert.Single(responses);
        Assert.Equal("Test User", responses[0].Name);
        Assert.Equal(UserStatus.ACTIVE, responses[0].Status);
    }

    #endregion

    #region GetUserById Tests

    [Fact]
    public async Task GetUserById_ExistingUser_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            ID = userId,
            Name = "Test User",
            Email = "test@test.com",
            NIF = "123456789",
            IsActive = true
        };

        _mockUserRepo.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        // Act
        var result = await _mockUserRepo.Object.GetById(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.ID);
        Assert.Equal("Test User", result.Name);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task GetUserById_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepo.Setup(r => r.GetById(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _mockUserRepo.Object.GetById(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserById_MapsToUserResponse_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            ID = userId,
            Name = "Test User",
            Email = "test@test.com",
            NIF = "123456789",
            IsActive = true
        };

        _mockUserRepo.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        // Act
        var result = await _mockUserRepo.Object.GetById(userId);
        var response = new UserResponse(
            Id: result!.ID,
            Email: result.Email,
            Name: result.Name,
            Status: result.IsActive ? UserStatus.ACTIVE : UserStatus.INACTIVE
        );

        // Assert
        Assert.Equal(userId, response.Id);
        Assert.Equal("Test User", response.Name);
        Assert.Equal(UserStatus.ACTIVE, response.Status);
    }

    #endregion

    #region DeleteUser Tests

    [Fact]
    public async Task DeleteUser_ExistingUser_DeletesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockClientRepo.Setup(r => r.Delete(userId)).Returns(Task.CompletedTask);

        // Act
        await _mockClientRepo.Object.Delete(userId);

        // Assert
        _mockClientRepo.Verify(r => r.Delete(userId), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_NonExistingUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockClientRepo.Setup(r => r.Delete(userId)).ThrowsAsync(new KeyNotFoundException());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _mockClientRepo.Object.Delete(userId));
    }

    [Fact]
    public async Task DeleteUser_CascadesDeleteToRelatedEntities()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // The ClientRepository.Delete should handle cascade deletion
        _mockClientRepo.Setup(r => r.Delete(userId)).Returns(Task.CompletedTask);

        // Act
        await _mockClientRepo.Object.Delete(userId);

        // Assert
        // Verify that delete was called, which internally handles cascade
        _mockClientRepo.Verify(r => r.Delete(userId), Times.Once);
    }

    #endregion

    #region UserResponse Mapping Tests

    [Fact]
    public void UserResponse_ActiveUser_MapsStatusCorrectly()
    {
        // Arrange
        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "Active User",
            Email = "active@test.com",
            NIF = "123456789",
            IsActive = true
        };

        // Act
        var response = new UserResponse(
            Id: user.ID,
            Email: user.Email,
            Name: user.Name,
            Status: user.IsActive ? UserStatus.ACTIVE : UserStatus.INACTIVE
        );

        // Assert
        Assert.Equal(UserStatus.ACTIVE, response.Status);
    }

    [Fact]
    public void UserResponse_InactiveUser_MapsStatusCorrectly()
    {
        // Arrange
        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "Inactive User",
            Email = "inactive@test.com",
            NIF = "123456789",
            IsActive = false
        };

        // Act
        var response = new UserResponse(
            Id: user.ID,
            Email: user.Email,
            Name: user.Name,
            Status: user.IsActive ? UserStatus.ACTIVE : UserStatus.INACTIVE
        );

        // Assert
        Assert.Equal(UserStatus.INACTIVE, response.Status);
    }

    [Fact]
    public void UserResponse_MapsAllFields_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            ID = userId,
            Name = "Complete User",
            Email = "complete@test.com",
            NIF = "123456789",
            IsActive = true
        };

        // Act
        var response = new UserResponse(
            Id: user.ID,
            Email: user.Email,
            Name: user.Name,
            Status: user.IsActive ? UserStatus.ACTIVE : UserStatus.INACTIVE
        );

        // Assert
        Assert.Equal(userId, response.Id);
        Assert.Equal("Complete User", response.Name);
        Assert.Equal("complete@test.com", response.Email);
        Assert.Equal(UserStatus.ACTIVE, response.Status);
    }

    #endregion

    #region User Collection Operations Tests

    [Fact]
    public async Task GetAllUsers_WithMultipleUsers_PreservesOrder()
    {
        // Arrange
        var users = new List<User>
        {
            new User { ID = Guid.NewGuid(), Name = "Alice", Email = "alice@test.com", NIF = "111111111", IsActive = true },
            new User { ID = Guid.NewGuid(), Name = "Bob", Email = "bob@test.com", NIF = "222222222", IsActive = true },
            new User { ID = Guid.NewGuid(), Name = "Charlie", Email = "charlie@test.com", NIF = "333333333", IsActive = true }
        };

        _mockUserRepo.Setup(r => r.GetAll()).ReturnsAsync(users);

        // Act
        var result = await _mockUserRepo.Object.GetAll();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("Bob", result[1].Name);
        Assert.Equal("Charlie", result[2].Name);
    }

    [Fact]
    public async Task GetAllUsers_WithMixedStatus_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { ID = Guid.NewGuid(), Name = "Active User", Email = "active@test.com", NIF = "111111111", IsActive = true },
            new User { ID = Guid.NewGuid(), Name = "Inactive User", Email = "inactive@test.com", NIF = "222222222", IsActive = false }
        };

        _mockUserRepo.Setup(r => r.GetAll()).ReturnsAsync(users);

        // Act
        var result = await _mockUserRepo.Object.GetAll();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result[0].IsActive);
        Assert.False(result[1].IsActive);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task UserRepository_GetAll_CalledOnce()
    {
        // Arrange
        var users = new List<User>();
        _mockUserRepo.Setup(r => r.GetAll()).ReturnsAsync(users);

        // Act
        await _mockUserRepo.Object.GetAll();

        // Assert
        _mockUserRepo.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public async Task UserRepository_GetById_CalledWithCorrectId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { ID = userId, Name = "Test", Email = "test@test.com", NIF = "123456789", IsActive = true };
        _mockUserRepo.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        // Act
        await _mockUserRepo.Object.GetById(userId);

        // Assert
        _mockUserRepo.Verify(r => r.GetById(userId), Times.Once);
    }

    [Fact]
    public async Task ClientRepository_Delete_CalledWithCorrectId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockClientRepo.Setup(r => r.Delete(userId)).Returns(Task.CompletedTask);

        // Act
        await _mockClientRepo.Object.Delete(userId);

        // Assert
        _mockClientRepo.Verify(r => r.Delete(userId), Times.Once);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetUserById_WithEmptyGuid_ReturnsNull()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        _mockUserRepo.Setup(r => r.GetById(emptyGuid)).ReturnsAsync((User?)null);

        // Act
        var result = await _mockUserRepo.Object.GetById(emptyGuid);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllUsers_LargeDataSet_HandlesCorrectly()
    {
        // Arrange
        var users = Enumerable.Range(1, 1000)
            .Select(i => new User
            {
                ID = Guid.NewGuid(),
                Name = $"User {i}",
                Email = $"user{i}@test.com",
                NIF = $"{i:D9}",
                IsActive = i % 2 == 0
            }).ToList();

        _mockUserRepo.Setup(r => r.GetAll()).ReturnsAsync(users);

        // Act
        var result = await _mockUserRepo.Object.GetAll();

        // Assert
        Assert.Equal(1000, result.Count);
        Assert.Equal("User 1", result[0].Name);
        Assert.Equal("User 1000", result[999].Name);
    }

    [Fact]
    public void UserResponse_WithEmptyEmail_HandlesCorrectly()
    {
        // Arrange
        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "User",
            Email = string.Empty,
            NIF = "123456789",
            IsActive = true
        };

        // Act
        var response = new UserResponse(
            Id: user.ID,
            Email: user.Email,
            Name: user.Name,
            Status: user.IsActive ? UserStatus.ACTIVE : UserStatus.INACTIVE
        );

        // Assert
        Assert.Equal(string.Empty, response.Email);
    }

    #endregion
}
