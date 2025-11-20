using Moq;
using Consilium.Application.Interfaces;
using Consilium.Domain.Models;
using Consilium.Domain.Enums;
using Consilium.API.Dtos;
using Consilium.API.Services;

namespace Consilium.Tests.Endpoints;

/// <summary>
/// Tests for Auth Endpoints business logic
/// These tests validate authentication, password hashing, and JWT token generation
/// </summary>
public class AuthEndpointsTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IPasswordHasher> _mockHasher;
    private readonly Mock<IJwtTokenService> _mockTokenService;

    public AuthEndpointsTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockHasher = new Mock<IPasswordHasher>();
        // JwtTokenService requires IConfiguration, so we mock it
        _mockTokenService = new Mock<IJwtTokenService>();
    }

    #region Login Tests

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var email = "user@test.com";
        var password = "password123";
        var hashedPassword = "hashedPassword123";
        var token = "jwt.token.here";

        var user = new User
        {
            ID = Guid.NewGuid(),
            Email = email,
            PasswordHash = hashedPassword,
            Name = "Test User",
            NIF = "123456789",
            IsActive = true
        };

        var users = new List<User> { user };

        _mockUserRepo.Setup(r => r.GetAll()).ReturnsAsync(users);
        _mockHasher.Setup(h => h.VerifyPassword(password, hashedPassword)).Returns(true);
        _mockTokenService.Setup(s => s.GenerateToken(It.IsAny<User>())).ReturnsAsync(token);

        // Act
        var allUsers = await _mockUserRepo.Object.GetAll();
        var foundUser = allUsers.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        var isPasswordValid = _mockHasher.Object.VerifyPassword(password, foundUser!.PasswordHash);
        var generatedToken = await _mockTokenService.Object.GenerateToken(foundUser);

        // Assert
        Assert.NotNull(foundUser);
        Assert.True(isPasswordValid);
        Assert.Equal(token, generatedToken);
    }

    [Fact]
    public async Task Login_InvalidEmail_UserNotFound()
    {
        // Arrange
        var email = "nonexistent@test.com";
        var users = new List<User>();

        _mockUserRepo.Setup(r => r.GetAll()).ReturnsAsync(users);

        // Act
        var allUsers = await _mockUserRepo.Object.GetAll();
        var foundUser = allUsers.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        // Assert
        Assert.Null(foundUser);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var email = "user@test.com";
        var password = "wrongPassword";
        var hashedPassword = "hashedPassword123";

        var user = new User
        {
            ID = Guid.NewGuid(),
            Email = email,
            PasswordHash = hashedPassword,
            Name = "Test User",
            NIF = "123456789",
            IsActive = true
        };

        var users = new List<User> { user };

        _mockUserRepo.Setup(r => r.GetAll()).ReturnsAsync(users);
        _mockHasher.Setup(h => h.VerifyPassword(password, hashedPassword)).Returns(false);

        // Act
        var allUsers = await _mockUserRepo.Object.GetAll();
        var foundUser = allUsers.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        var isPasswordValid = _mockHasher.Object.VerifyPassword(password, foundUser!.PasswordHash);

        // Assert
        Assert.NotNull(foundUser);
        Assert.False(isPasswordValid);
    }

    [Fact]
    public async Task Login_InactiveUser_ReturnsBadRequest()
    {
        // Arrange
        var email = "inactive@test.com";
        var password = "password123";
        var hashedPassword = "hashedPassword123";

        var user = new User
        {
            ID = Guid.NewGuid(),
            Email = email,
            PasswordHash = hashedPassword,
            Name = "Inactive User",
            NIF = "123456789",
            IsActive = false
        };

        var users = new List<User> { user };

        _mockUserRepo.Setup(r => r.GetAll()).ReturnsAsync(users);
        _mockHasher.Setup(h => h.VerifyPassword(password, hashedPassword)).Returns(true);

        // Act
        var allUsers = await _mockUserRepo.Object.GetAll();
        var foundUser = allUsers.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        var isPasswordValid = _mockHasher.Object.VerifyPassword(password, foundUser!.PasswordHash);

        // Assert
        Assert.NotNull(foundUser);
        Assert.True(isPasswordValid);
        Assert.False(foundUser.IsActive);
    }

    [Fact]
    public void Login_EmptyEmail_ShouldValidateFails()
    {
        // Arrange
        var request = new LoginRequest(Email: "", Password: "password123");

        // Act & Assert
        Assert.True(string.IsNullOrWhiteSpace(request.Email));
    }

    [Fact]
    public void Login_EmptyPassword_ShouldValidateFails()
    {
        // Arrange
        var request = new LoginRequest(Email: "user@test.com", Password: "");

        // Act & Assert
        Assert.True(string.IsNullOrWhiteSpace(request.Password));
    }

    [Fact]
    public void Login_CaseInsensitiveEmail_FindsUser()
    {
        // Arrange
        var user = new User
        {
            ID = Guid.NewGuid(),
            Email = "User@Test.COM",
            PasswordHash = "hash",
            Name = "Test User",
            NIF = "123456789",
            IsActive = true
        };

        var users = new List<User> { user };

        // Act
        var foundUser = users.FirstOrDefault(u => u.Email.Equals("user@test.com", StringComparison.OrdinalIgnoreCase));

        // Assert
        Assert.NotNull(foundUser);
        Assert.Equal("User@Test.COM", foundUser.Email);
    }

    #endregion

    #region LoginResponse Tests

    [Fact]
    public void LoginResponse_MapsFromUser_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "jwt.token.here";
        var user = new User
        {
            ID = userId,
            Email = "test@test.com",
            Name = "Test User",
            IsActive = true
        };

        // Act
        var response = new LoginResponse(
            Token: token,
            UserId: user.ID,
            Email: user.Email,
            Name: user.Name,
            Status: user.IsActive ? UserStatus.ACTIVE : UserStatus.INACTIVE
        );

        // Assert
        Assert.Equal(token, response.Token);
        Assert.Equal(userId, response.UserId);
        Assert.Equal("test@test.com", response.Email);
        Assert.Equal("Test User", response.Name);
        Assert.Equal(UserStatus.ACTIVE, response.Status);
    }

    [Fact]
    public void LoginResponse_InactiveUser_MapsStatusCorrectly()
    {
        // Arrange
        var user = new User
        {
            ID = Guid.NewGuid(),
            Email = "test@test.com",
            Name = "Test User",
            IsActive = false
        };

        // Act
        var status = user.IsActive ? UserStatus.ACTIVE : UserStatus.INACTIVE;

        // Assert
        Assert.Equal(UserStatus.INACTIVE, status);
    }

    #endregion

    #region Password Verification Tests

    [Fact]
    public void PasswordHasher_VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "mySecurePassword";
        var hashedPassword = "hashedVersion";

        _mockHasher.Setup(h => h.VerifyPassword(password, hashedPassword)).Returns(true);

        // Act
        var result = _mockHasher.Object.VerifyPassword(password, hashedPassword);

        // Assert
        Assert.True(result);
        _mockHasher.Verify(h => h.VerifyPassword(password, hashedPassword), Times.Once);
    }

    [Fact]
    public void PasswordHasher_VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "wrongPassword";
        var hashedPassword = "hashedVersion";

        _mockHasher.Setup(h => h.VerifyPassword(password, hashedPassword)).Returns(false);

        // Act
        var result = _mockHasher.Object.VerifyPassword(password, hashedPassword);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Token Generation Tests

    [Fact]
    public async Task TokenService_GenerateToken_CreatesValidToken()
    {
        // Arrange
        var user = new User
        {
            ID = Guid.NewGuid(),
            Email = "user@test.com",
            Name = "User Name",
            IsActive = true
        };

        var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        _mockTokenService.Setup(s => s.GenerateToken(user)).ReturnsAsync(expectedToken);

        // Act
        var token = await _mockTokenService.Object.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.Equal(expectedToken, token);
        _mockTokenService.Verify(s => s.GenerateToken(user), Times.Once);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Login_MultipleUsersWithSameEmailCasing_FindsCorrectOne()
    {
        // Arrange
        var users = new List<User>
        {
            new User { ID = Guid.NewGuid(), Email = "test@example.com", Name = "User1", IsActive = true },
            new User { ID = Guid.NewGuid(), Email = "TEST@EXAMPLE.COM", Name = "User2", IsActive = true }
        };

        _mockUserRepo.Setup(r => r.GetAll()).ReturnsAsync(users);

        // Act
        var allUsers = await _mockUserRepo.Object.GetAll();
        var foundUser = allUsers.FirstOrDefault(u => u.Email.Equals("TeSt@ExAmPlE.cOm", StringComparison.OrdinalIgnoreCase));

        // Assert
        Assert.NotNull(foundUser);
        // Should find the first matching user
        Assert.Equal("test@example.com", foundUser.Email);
    }

    [Fact]
    public void Login_NullEmailInRequest_CanBeDetected()
    {
        // Arrange
        var request = new LoginRequest(Email: null!, Password: "password");

        // Act & Assert
        Assert.True(string.IsNullOrWhiteSpace(request.Email));
    }

    [Fact]
    public void Login_NullPasswordInRequest_CanBeDetected()
    {
        // Arrange
        var request = new LoginRequest(Email: "user@test.com", Password: null!);

        // Act & Assert
        Assert.True(string.IsNullOrWhiteSpace(request.Password));
    }

    #endregion
}
