using Microsoft.EntityFrameworkCore;
using Consilium.Domain.Models;
using Consilium.Infrastructure.Data;
using Consilium.Infrastructure.Repositories;

namespace Consilium.Tests.Repositories;

public class ClientRepositoryTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Create_ValidClientAndUser_ReturnsCreatedClient()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new ClientRepository(context);

        var user = new User
        {
            Name = "John Doe",
            Email = "john@test.com",
            NIF = "123456789",
            PasswordHash = "hashedpassword",
            IsActive = true
        };

        var client = new Client
        {
            Address = "123 Main St"
        };

        // Act
        var result = await repository.Create(user, client);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(client.Address, result.Address);
        Assert.Equal(user.ID, result.ID);
        Assert.NotEqual(Guid.Empty, result.ID);
    }

    [Fact]
    public async Task GetById_ExistingClient_ReturnsClientWithUser()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new ClientRepository(context);

        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "Jane Doe",
            Email = "jane@test.com",
            NIF = "987654321",
            PasswordHash = "hashedpassword",
            IsActive = true
        };

        var client = new Client
        {
            ID = user.ID,
            Address = "456 Oak Ave"
        };

        await context.Users.AddAsync(user);
        await context.Clients.AddAsync(client);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetById(client.ID);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(client.ID, result.ID);
        Assert.NotNull(result.User);
        Assert.Equal(user.Name, result.User.Name);
    }

    [Fact]
    public async Task GetById_NonExistingClient_ReturnsNull()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new ClientRepository(context);
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await repository.GetById(nonExistingId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAll_WithSearchFilter_ReturnsFilteredClients()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new ClientRepository(context);

        var user1 = new User { ID = Guid.NewGuid(), Name = "Alice Smith", Email = "alice@test.com", NIF = "111111111", IsActive = true };
        var client1 = new Client { ID = user1.ID, Address = "123 St" };

        var user2 = new User { ID = Guid.NewGuid(), Name = "Bob Jones", Email = "bob@test.com", NIF = "222222222", IsActive = true };
        var client2 = new Client { ID = user2.ID, Address = "456 Ave" };

        await context.Users.AddRangeAsync(user1, user2);
        await context.Clients.AddRangeAsync(client1, client2);
        await context.SaveChangesAsync();

        // Act
        var (clients, totalCount) = await repository.GetAll("Alice", null, 1, 10, null, null);

        // Assert
        Assert.Single(clients);
        Assert.Equal(1, totalCount);
        Assert.Equal("Alice Smith", clients[0].User.Name);
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_ReturnsActiveClients()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new ClientRepository(context);

        var user1 = new User { ID = Guid.NewGuid(), Name = "Active Client", Email = "active@test.com", NIF = "111111111", IsActive = true };
        var client1 = new Client { ID = user1.ID, Address = "123 St" };

        var user2 = new User { ID = Guid.NewGuid(), Name = "Inactive Client", Email = "inactive@test.com", NIF = "222222222", IsActive = false };
        var client2 = new Client { ID = user2.ID, Address = "456 Ave" };

        await context.Users.AddRangeAsync(user1, user2);
        await context.Clients.AddRangeAsync(client1, client2);
        await context.SaveChangesAsync();

        // Act
        var (clients, totalCount) = await repository.GetAll(null, "ACTIVE", 1, 10, null, null);

        // Assert
        Assert.Single(clients);
        Assert.Equal(1, totalCount);
        Assert.True(clients[0].User.IsActive);
    }

    [Fact]
    public async Task GetAll_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new ClientRepository(context);

        for (int i = 1; i <= 5; i++)
        {
            var user = new User { ID = Guid.NewGuid(), Name = $"Client {i}", Email = $"client{i}@test.com", NIF = $"{i}11111111", IsActive = true };
            var client = new Client { ID = user.ID, Address = $"{i} St" };
            await context.Users.AddAsync(user);
            await context.Clients.AddAsync(client);
        }
        await context.SaveChangesAsync();

        // Act
        var (clients, totalCount) = await repository.GetAll(null, null, 2, 2, null, null);

        // Assert
        Assert.Equal(2, clients.Count);
        Assert.Equal(5, totalCount);
    }

    [Fact]
    public async Task UpdateClientAndUser_ValidData_UpdatesSuccessfully()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new ClientRepository(context);

        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "Old Name",
            Email = "old@test.com",
            NIF = "123456789",
            PasswordHash = "oldhash",
            IsActive = true
        };

        var client = new Client
        {
            ID = user.ID,
            Address = "Old Address"
        };

        await context.Users.AddAsync(user);
        await context.Clients.AddAsync(client);
        await context.SaveChangesAsync();

        var userUpdates = new User { Name = "New Name", Email = "new@test.com" };
        var clientUpdates = new Client { Address = "New Address" };

        // Act
        var result = await repository.UpdateClientAndUser(client.ID, clientUpdates, userUpdates, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.User.Name);
        Assert.Equal("new@test.com", result.User.Email);
        Assert.Equal("New Address", result.Address);
    }

    [Fact]
    public async Task UpdateClientAndUser_NonExistingClient_ReturnsNull()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new ClientRepository(context);

        var nonExistingId = Guid.NewGuid();
        var userUpdates = new User { Name = "New Name" };
        var clientUpdates = new Client { Address = "New Address" };

        // Act
        var result = await repository.UpdateClientAndUser(nonExistingId, clientUpdates, userUpdates, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_ExistingClient_DeletesSuccessfully()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new ClientRepository(context);

        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "To Delete",
            Email = "delete@test.com",
            NIF = "123456789",
            PasswordHash = "hash",
            IsActive = true
        };

        var client = new Client
        {
            ID = user.ID,
            Address = "Delete Address"
        };

        await context.Users.AddAsync(user);
        await context.Clients.AddAsync(client);
        await context.SaveChangesAsync();

        // Act
        await repository.Delete(client.ID);

        // Assert
        var deletedUser = await context.Users.FindAsync(user.ID);
        var deletedClient = await context.Clients.FindAsync(client.ID);
        Assert.Null(deletedUser);
        Assert.Null(deletedClient);
    }

    [Fact]
    public async Task Delete_NonExistingClient_ThrowsKeyNotFoundException()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new ClientRepository(context);
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.Delete(nonExistingId));
    }
}
