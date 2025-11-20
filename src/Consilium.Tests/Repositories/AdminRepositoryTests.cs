using Microsoft.EntityFrameworkCore;
using Consilium.Domain.Models;
using Consilium.Infrastructure.Data;
using Consilium.Infrastructure.Repositories;

namespace Consilium.Tests.Repositories;

public class AdminRepositoryTests
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
    public async Task Create_ValidAdminAndUser_ReturnsCreatedAdmin()
    {
        await using var context = GetInMemoryDbContext();
        var repository = new AdminRepository(context);

        var user = new User
        {
            Name = "Admin User",
            Email = "admin@test.com",
            NIF = "999999999",
            PasswordHash = "hash",
            IsActive = true
        };

        var admin = new Admin { StartedAt = DateTime.UtcNow };

        var result = await repository.Create(user, admin);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.ID);
        Assert.True(result.User.IsActive);
    }

    [Fact]
    public async Task GetById_ExistingAdmin_ReturnsAdminWithUser()
    {
        await using var context = GetInMemoryDbContext();
        var repository = new AdminRepository(context);

        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "Admin1",
            Email = "a1@test.com",
            NIF = "123456789",
            PasswordHash = "hash",
            IsActive = true
        };
        var admin = new Admin { ID = user.ID, StartedAt = DateTime.UtcNow };

        await context.Users.AddAsync(user);
        await context.Admins.AddAsync(admin);
        await context.SaveChangesAsync();

        var result = await repository.GetById(admin.ID);
        Assert.NotNull(result);
        Assert.Equal(admin.ID, result.ID);
        Assert.NotNull(result.User);
        Assert.Equal(user.Name, result.User.Name);
    }

    [Fact]
    public async Task Delete_ExistingAdmin_DeletesSuccessfully()
    {
        await using var context = GetInMemoryDbContext();
        var repository = new AdminRepository(context);

        var user = new User { ID = Guid.NewGuid(), Name = "X", Email = "x@test.com", NIF = "111222333", PasswordHash = "hash", IsActive = true };
        var admin = new Admin { ID = user.ID, StartedAt = DateTime.UtcNow };
        var phone = new Phone { ID = Guid.NewGuid(), UserID = user.ID, Number = "1111", CountryCode = 351, IsMain = true };

        await context.Users.AddAsync(user);
        await context.Admins.AddAsync(admin);
        await context.Phones.AddAsync(phone);
        await context.SaveChangesAsync();

        await repository.Delete(admin.ID);

        var deletedUser = await context.Users.FindAsync(user.ID);
        var deletedAdmin = await context.Admins.FindAsync(admin.ID);
        var deletedPhone = await context.Phones.FindAsync(phone.ID);
        Assert.Null(deletedUser);
        Assert.Null(deletedAdmin);
        Assert.Null(deletedPhone);
    }

    [Fact]
    public async Task Delete_NonExistingAdmin_ThrowsKeyNotFoundException()
    {
        await using var context = GetInMemoryDbContext();
        var repository = new AdminRepository(context);
        var nonExistingId = Guid.NewGuid();
        await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.Delete(nonExistingId));
    }

    [Fact]
    public async Task UpdateAdminAndUser_UpdatesUserAndAddsOrUpdatesPhone()
    {
        await using var context = GetInMemoryDbContext();
        var repository = new AdminRepository(context);

        var user = new User { ID = Guid.NewGuid(), Name = "Old", Email = "old@test.com", NIF = "111111111", PasswordHash = "hash", IsActive = true };
        var admin = new Admin { ID = user.ID, StartedAt = DateTime.UtcNow };
        var existingPhone = new Phone { ID = Guid.NewGuid(), UserID = user.ID, Number = "3333", CountryCode = 351, IsMain = true };

        await context.Users.AddAsync(user);
        await context.Admins.AddAsync(admin);
        await context.Phones.AddAsync(existingPhone);
        await context.SaveChangesAsync();

        var userUpdates = new User { Name = "NewName", Email = "new@test.com", Phones = new List<Phone> { new Phone { Number = "6666", CountryCode = 351, IsMain = true } } };

        var result = await repository.UpdateAdminAndUser(admin.ID, new Admin(), userUpdates);

        Assert.NotNull(result);
        Assert.Equal("NewName", result.User.Name);
        Assert.Equal("new@test.com", result.User.Email);
        // ensure the existing main phone was updated
        var phone = result.User.Phones.FirstOrDefault(p => p.ID == existingPhone.ID);
        Assert.NotNull(phone);
        Assert.Equal("6666", phone.Number);
    }

    [Fact]
    public async Task UpdateAdminAndUser_WithNoMainPhone_AddsNewPhone()
    {
        await using var context = GetInMemoryDbContext();
        var repository = new AdminRepository(context);

        var user = new User { ID = Guid.NewGuid(), Name = "Old", Email = "old@test.com", NIF = "111111112", PasswordHash = "hash", IsActive = true };
        var admin = new Admin { ID = user.ID, StartedAt = DateTime.UtcNow };

        await context.Users.AddAsync(user);
        await context.Admins.AddAsync(admin);
        await context.SaveChangesAsync();

        var userUpdates = new User { Phones = new List<Phone> { new Phone { Number = "7777", CountryCode = 351, IsMain = true } } };

        var result = await repository.UpdateAdminAndUser(admin.ID, new Admin(), userUpdates);
        Assert.NotNull(result);
        Assert.NotEmpty(result.User.Phones);
        Assert.Equal("7777", result.User.Phones.First().Number);
    }
}
