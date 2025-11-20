using Microsoft.EntityFrameworkCore;
using Consilium.Domain.Models;
using Consilium.Infrastructure.Data;
using Consilium.Infrastructure.Repositories;

namespace Consilium.Tests.Repositories;

public class LawyerRepositoryTests
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
    public async Task Create_ValidLawyerAndUser_ReturnsCreatedLawyer()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new LawyerRepository(context);

        var user = new User
        {
            Name = "Jane Attorney",
            Email = "jane@law.com",
            NIF = "987654321",
            PasswordHash = "hashedpassword",
            IsActive = true
        };

        var lawyer = new Lawyer
        {
            ProfessionalRegister = "LAW12345"
        };

        // Act
        var result = await repository.Create(user, lawyer);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lawyer.ProfessionalRegister, result.ProfessionalRegister);
        Assert.Equal(user.ID, result.ID);
        Assert.NotEqual(Guid.Empty, result.ID);
    }

    [Fact]
    public async Task GetById_ExistingLawyer_ReturnsLawyerWithUser()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new LawyerRepository(context);

        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "John Attorney",
            Email = "john@law.com",
            NIF = "123456789",
            PasswordHash = "hashedpassword",
            IsActive = true
        };

        var lawyer = new Lawyer
        {
            ID = user.ID,
            ProfessionalRegister = "LAW67890"
        };

        await context.Users.AddAsync(user);
        await context.Lawyers.AddAsync(lawyer);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetById(lawyer.ID);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lawyer.ID, result.ID);
        Assert.NotNull(result.User);
        Assert.Equal(user.Name, result.User.Name);
        Assert.Equal("LAW67890", result.ProfessionalRegister);
    }

    [Fact]
    public async Task GetById_NonExistingLawyer_ReturnsNull()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new LawyerRepository(context);
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await repository.GetById(nonExistingId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAll_WithSearchFilter_ReturnsFilteredLawyers()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new LawyerRepository(context);

        var user1 = new User { ID = Guid.NewGuid(), Name = "Alice Lawyer", Email = "alice@law.com", NIF = "111111111", IsActive = true };
        var lawyer1 = new Lawyer { ID = user1.ID, ProfessionalRegister = "LAW111" };

        var user2 = new User { ID = Guid.NewGuid(), Name = "Bob Lawyer", Email = "bob@law.com", NIF = "222222222", IsActive = true };
        var lawyer2 = new Lawyer { ID = user2.ID, ProfessionalRegister = "LAW222" };

        await context.Users.AddRangeAsync(user1, user2);
        await context.Lawyers.AddRangeAsync(lawyer1, lawyer2);
        await context.SaveChangesAsync();

        // Act
        var (lawyers, totalCount) = await repository.GetAll("Alice", null, 1, 10, null, null);

        // Assert
        Assert.Single(lawyers);
        Assert.Equal(1, totalCount);
        Assert.Equal("Alice Lawyer", lawyers[0].User.Name);
    }

    [Fact]
    public async Task GetAll_SearchByProfessionalRegister_ReturnsCorrectLawyer()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new LawyerRepository(context);

        var user1 = new User { ID = Guid.NewGuid(), Name = "Alice Lawyer", Email = "alice@law.com", NIF = "111111111", IsActive = true };
        var lawyer1 = new Lawyer { ID = user1.ID, ProfessionalRegister = "LAW111" };

        var user2 = new User { ID = Guid.NewGuid(), Name = "Bob Lawyer", Email = "bob@law.com", NIF = "222222222", IsActive = true };
        var lawyer2 = new Lawyer { ID = user2.ID, ProfessionalRegister = "LAW222" };

        await context.Users.AddRangeAsync(user1, user2);
        await context.Lawyers.AddRangeAsync(lawyer1, lawyer2);
        await context.SaveChangesAsync();

        // Act
        var (lawyers, totalCount) = await repository.GetAll("LAW222", null, 1, 10, null, null);

        // Assert
        Assert.Single(lawyers);
        Assert.Equal(1, totalCount);
        Assert.Equal("LAW222", lawyers[0].ProfessionalRegister);
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_ReturnsActiveLawyers()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new LawyerRepository(context);

        var user1 = new User { ID = Guid.NewGuid(), Name = "Active Lawyer", Email = "active@law.com", NIF = "111111111", IsActive = true };
        var lawyer1 = new Lawyer { ID = user1.ID, ProfessionalRegister = "LAW111" };

        var user2 = new User { ID = Guid.NewGuid(), Name = "Inactive Lawyer", Email = "inactive@law.com", NIF = "222222222", IsActive = false };
        var lawyer2 = new Lawyer { ID = user2.ID, ProfessionalRegister = "LAW222" };

        await context.Users.AddRangeAsync(user1, user2);
        await context.Lawyers.AddRangeAsync(lawyer1, lawyer2);
        await context.SaveChangesAsync();

        // Act
        var (lawyers, totalCount) = await repository.GetAll(null, "ACTIVE", 1, 10, null, null);

        // Assert
        Assert.Single(lawyers);
        Assert.Equal(1, totalCount);
        Assert.True(lawyers[0].User.IsActive);
    }

    [Fact]
    public async Task GetAll_SortByRegister_ReturnsSortedLawyers()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new LawyerRepository(context);

        var user1 = new User { ID = Guid.NewGuid(), Name = "Lawyer A", Email = "a@law.com", NIF = "111111111", IsActive = true };
        var lawyer1 = new Lawyer { ID = user1.ID, ProfessionalRegister = "LAW333" };

        var user2 = new User { ID = Guid.NewGuid(), Name = "Lawyer B", Email = "b@law.com", NIF = "222222222", IsActive = true };
        var lawyer2 = new Lawyer { ID = user2.ID, ProfessionalRegister = "LAW111" };

        await context.Users.AddRangeAsync(user1, user2);
        await context.Lawyers.AddRangeAsync(lawyer1, lawyer2);
        await context.SaveChangesAsync();

        // Act
        var (lawyers, totalCount) = await repository.GetAll(null, null, 1, 10, "register", "asc");

        // Assert
        Assert.Equal(2, lawyers.Count);
        Assert.Equal("LAW111", lawyers[0].ProfessionalRegister);
        Assert.Equal("LAW333", lawyers[1].ProfessionalRegister);
    }

    [Fact]
    public async Task UpdateLawyerAndUser_ValidData_UpdatesSuccessfully()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new LawyerRepository(context);

        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "Old Name",
            Email = "old@law.com",
            NIF = "123456789",
            PasswordHash = "oldhash",
            IsActive = true
        };

        var lawyer = new Lawyer
        {
            ID = user.ID,
            ProfessionalRegister = "LAW000"
        };

        await context.Users.AddAsync(user);
        await context.Lawyers.AddAsync(lawyer);
        await context.SaveChangesAsync();

        var userUpdates = new User { Name = "New Name", Email = "new@law.com" };
        var lawyerUpdates = new Lawyer { ProfessionalRegister = "LAW999" };

        // Act
        var result = await repository.UpdateLawyerAndUser(lawyer.ID, lawyerUpdates, userUpdates);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.User.Name);
        Assert.Equal("new@law.com", result.User.Email);
        Assert.Equal("LAW999", result.ProfessionalRegister);
    }

    [Fact]
    public async Task UpdateLawyerAndUser_NonExistingLawyer_ReturnsNull()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new LawyerRepository(context);

        var nonExistingId = Guid.NewGuid();
        var userUpdates = new User { Name = "New Name" };
        var lawyerUpdates = new Lawyer { ProfessionalRegister = "LAW999" };

        // Act
        var result = await repository.UpdateLawyerAndUser(nonExistingId, lawyerUpdates, userUpdates);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_ExistingLawyer_DeletesSuccessfully()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new LawyerRepository(context);

        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "To Delete",
            Email = "delete@law.com",
            NIF = "123456789",
            PasswordHash = "hash",
            IsActive = true
        };

        var lawyer = new Lawyer
        {
            ID = user.ID,
            ProfessionalRegister = "LAW000"
        };

        await context.Users.AddAsync(user);
        await context.Lawyers.AddAsync(lawyer);
        await context.SaveChangesAsync();

        // Act
        await repository.Delete(lawyer.ID);

        // Assert
        var deletedUser = await context.Users.FindAsync(user.ID);
        var deletedLawyer = await context.Lawyers.FindAsync(lawyer.ID);
        Assert.Null(deletedUser);
        Assert.Null(deletedLawyer);
    }

    [Fact]
    public async Task Delete_NonExistingLawyer_ThrowsKeyNotFoundException()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var repository = new LawyerRepository(context);
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.Delete(nonExistingId));
    }

    [Fact]
    public async Task Delete_Lawyer_WithActiveProcesses_ThrowsInvalidOperationException()
    {
        await using var context = GetInMemoryDbContext();
        var repository = new LawyerRepository(context);

        // Create lawyer and related entities
        var user = new User { ID = Guid.NewGuid(), Name = "Lawyer", Email = "lawyer@test", NIF = "222222222", PasswordHash = "x", IsActive = true };
        var lawyer = new Lawyer { ID = user.ID, ProfessionalRegister = "PR-1" };
        var clientUser = new User { ID = Guid.NewGuid(), Name = "Client", Email = "client@test", NIF = "111111111", PasswordHash = "x", IsActive = true };
        var client = new Client { ID = clientUser.ID, Address = "somewhere" };

        await context.Users.AddRangeAsync(user, clientUser);
        await context.Clients.AddAsync(client);
        await context.Lawyers.AddAsync(lawyer);

        var ptype = new Consilium.Domain.Models.ProcessType { Name = "Civil" };
        var pphase = new Consilium.Domain.Models.ProcessPhase { Name = "Initial" };
        var pstatus = new Consilium.Domain.Models.ProcessStatus { Name = "Open", IsDefault = true };
        await context.ProcessTypes.AddAsync(ptype);
        await context.ProcessPhases.AddAsync(pphase);
        await context.ProcessStatuses.AddAsync(pstatus);
        await context.SaveChangesAsync();

        var typePhase = new Consilium.Domain.Models.ProcessTypePhase { ProcessPhaseId = pphase.Id, ProcessTypeId = ptype.Id, TypePhaseOrder = 1 };
        await context.ProcessTypePhases.AddAsync(typePhase);
        await context.SaveChangesAsync();

        // Create active process for lawyer
        var process = new Process { Name = "P1", Number = "P-1", ClientId = client.ID, LawyerId = lawyer.ID, Priority = 1, CourtInfo = "Court" , ProcessTypePhaseId = typePhase.Id, ProcessStatusId = pstatus.Id };
        await context.Processes.AddAsync(process);
        await context.SaveChangesAsync();

        // Trying to delete should throw
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.Delete(lawyer.ID));
    }
}
