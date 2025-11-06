using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Consilium.API.Services;
using Consilium.Domain.Models;
using Consilium.Infrastructure.Data;

namespace Consilium.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AppDbContext _context;
    private readonly JwtTokenService _jwtTokenService;

    public JwtTokenServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        // Setup configuration mock
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Jwt:Key"]).Returns("CacZyToeKnj525lN/bKishMbPLXvl+YXDg0eNKTccMw=");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("https://localhost:8080");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("http://localhost:4200");

        _jwtTokenService = new JwtTokenService(_configurationMock.Object, _context);
    }

    [Fact]
    public async Task GenerateToken_WithClientUser_ReturnsTokenWithClientRole()
    {
        // Arrange
        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "John Client",
            Email = "client@test.com",
            NIF = "123456789",
            IsActive = true
        };

        var client = new Client
        {
            ID = user.ID,
            Address = "123 Main St"
        };

        await _context.Users.AddAsync(user);
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var token = await _jwtTokenService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Verify all required claims
        Assert.Equal(user.ID.ToString(), jwtToken.Claims.First(c => c.Type == "user_id").Value);
        Assert.Equal(user.Name, jwtToken.Claims.First(c => c.Type == "username").Value);
        Assert.Equal("Client", jwtToken.Claims.First(c => c.Type == "role").Value);
        Assert.Equal(user.Email, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(user.NIF, jwtToken.Claims.First(c => c.Type == "nif").Value);
        Assert.NotNull(jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti));

        // Verify issuer and audience
        Assert.Equal("https://localhost:8080", jwtToken.Issuer);
        Assert.Contains("http://localhost:4200", jwtToken.Audiences);

        // Verify expiration (should be ~8 hours from now)
        var expectedExpiration = DateTime.UtcNow.AddHours(8);
        var actualExpiration = jwtToken.ValidTo;
        Assert.True(Math.Abs((expectedExpiration - actualExpiration).TotalMinutes) < 1);
    }

    [Fact]
    public async Task GenerateToken_WithLawyerUser_ReturnsTokenWithLawyerRole()
    {
        // Arrange
        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "Jane Lawyer",
            Email = "lawyer@test.com",
            NIF = "987654321",
            IsActive = true
        };

        var lawyer = new Lawyer
        {
            ID = user.ID,
            ProfessionalRegister = "LAW12345"
        };

        await _context.Users.AddAsync(user);
        await _context.Lawyers.AddAsync(lawyer);
        await _context.SaveChangesAsync();

        // Act
        var token = await _jwtTokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal("Lawyer", jwtToken.Claims.First(c => c.Type == "role").Value);
    }

    [Fact]
    public async Task GenerateToken_WithAdminUser_ReturnsTokenWithAdminRole()
    {
        // Arrange
        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "Bob Admin",
            Email = "admin@test.com",
            NIF = "555666777",
            IsActive = true
        };

        var admin = new Admin
        {
            ID = user.ID,
            StartedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
        };

        await _context.Users.AddAsync(user);
        await _context.Admins.AddAsync(admin);
        await _context.SaveChangesAsync();

        // Act
        var token = await _jwtTokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal("Admin", jwtToken.Claims.First(c => c.Type == "role").Value);
    }

    [Fact]
    public async Task GenerateToken_WithNoRoleAssociation_ReturnsTokenWithUserRole()
    {
        // Arrange
        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "Generic User",
            Email = "user@test.com",
            NIF = "111222333",
            IsActive = true
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var token = await _jwtTokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal("User", jwtToken.Claims.First(c => c.Type == "role").Value);
    }

    [Fact]
    public async Task GenerateToken_VerifyJtiIsUnique()
    {
        // Arrange
        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@test.com",
            NIF = "123456789",
            IsActive = true
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var token1 = await _jwtTokenService.GenerateToken(user);
        var token2 = await _jwtTokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);

        var jti1 = jwtToken1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = jwtToken2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        Assert.NotEqual(jti1, jti2);
    }
}
