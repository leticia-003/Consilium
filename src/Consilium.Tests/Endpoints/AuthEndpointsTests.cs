using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Consilium.Infrastructure.Data;
using Consilium.Domain.Models;
using Consilium.API.Dtos;
using Consilium.Application.Interfaces;
using Consilium.Infrastructure.Services;
using Consilium.API.Services;

namespace Consilium.Tests.Endpoints;

public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                // Replace AppDbContext with in-memory
                var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) || d.ServiceType == typeof(AppDbContext)).ToList();
                foreach (var d in descriptors)
                    services.Remove(d);

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                });

                // Register real password hasher
                services.AddTransient<IPasswordHasher, PasswordHasher>();

                // Ensure JwtTokenService registers properly and configuration is available (defaults used if missing)
            });
        });
    }

    private async Task SeedUsers(AppDbContext db, string email, string rawPassword, bool isActive = true)
    {
        var hasher = new PasswordHasher();
        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "Test",
            Email = email,
            NIF = "123456789",
            IsActive = isActive
        };
        user.PasswordHash = hasher.HashPassword(rawPassword);
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Login_MissingEmail_ReturnsBadRequest()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) || d.ServiceType == typeof(AppDbContext)).ToList();
                foreach (var d in descriptors)
                    services.Remove(d);
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                });
                services.AddTransient<IPasswordHasher, PasswordHasher>();
            });
        });
        var client = factory.CreateClient();
        var req = new LoginRequest(Email: "", Password: "pwd");
        var resp = await client.PostAsJsonAsync("/api/auth/login", req);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Login_MissingPassword_ReturnsBadRequest()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) || d.ServiceType == typeof(AppDbContext)).ToList();
                foreach (var d in descriptors)
                    services.Remove(d);
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                });
                services.AddTransient<IPasswordHasher, PasswordHasher>();
            });
        });
        var client = factory.CreateClient();
        var req = new LoginRequest(Email: "user@test.com", Password: "");
        var resp = await client.PostAsJsonAsync("/api/auth/login", req);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) || d.ServiceType == typeof(AppDbContext)).ToList();
                foreach (var d in descriptors)
                    services.Remove(d);
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                });
                services.AddTransient<IPasswordHasher, PasswordHasher>();
            });
        });
        var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedUsers(db, "user@test.com", "correctpwd");
        }
        var req = new LoginRequest(Email: "user@test.com", Password: "wrongpwd");
        var resp = await client.PostAsJsonAsync("/api/auth/login", req);
        Assert.Equal((HttpStatusCode)401, resp.StatusCode);
    }

    [Fact]
    public async Task Login_InactiveUser_ReturnsBadRequest()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) || d.ServiceType == typeof(AppDbContext)).ToList();
                foreach (var d in descriptors)
                    services.Remove(d);
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                });
                services.AddTransient<IPasswordHasher, PasswordHasher>();
            });
        });
        var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedUsers(db, "inactive@test.com", "pwd", isActive: false);
        }
        var req = new LoginRequest(Email: "inactive@test.com", Password: "pwd");
        var resp = await client.PostAsJsonAsync("/api/auth/login", req);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) || d.ServiceType == typeof(AppDbContext)).ToList();
                foreach (var d in descriptors)
                    services.Remove(d);
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                });
                services.AddTransient<IPasswordHasher, PasswordHasher>();
            });
        });
        var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedUsers(db, "valid@test.com", "pwd");
            // Verify that the password hash was stored and matches
            var user = db.Users.First(u => u.Email == "valid@test.com");
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var ok = hasher.VerifyPassword("pwd", user.PasswordHash);
            Assert.True(ok, "Seeded password should verify with registered IPasswordHasher");
        }

        var req = new LoginRequest(Email: "valid@test.com", Password: "pwd");
        var resp = await client.PostAsJsonAsync("/api/auth/login", req);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(json);
        Assert.False(string.IsNullOrWhiteSpace(json!.Token));
        Assert.Equal("valid@test.com", json.Email);
    }
}
