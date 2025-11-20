using Microsoft.EntityFrameworkCore;
using Consilium.Domain.Models;
using Consilium.Infrastructure.Data;
using Consilium.Infrastructure.Repositories;

namespace Consilium.Tests.Repositories;

public class ProcessRepositoryTests
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
    public async Task Create_Get_Update_Delete_ProcessFlow()
    {
        await using var context = GetInMemoryDbContext();
        var repo = new ProcessRepository(context);

        // Seed required related entities
        var clientUser = new User { ID = Guid.NewGuid(), Name = "Client Test", Email = "client@test", NIF = "111111111", PasswordHash = "x", IsActive = true };
        var lawyerUser = new User { ID = Guid.NewGuid(), Name = "Lawyer Test", Email = "lawyer@test", NIF = "222222222", PasswordHash = "x", IsActive = true };
        var client = new Client { ID = clientUser.ID, Address = "somewhere" };
        var lawyer = new Lawyer { ID = lawyerUser.ID, ProfessionalRegister = "ABC-1" };

        await context.Users.AddRangeAsync(clientUser, lawyerUser);
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

        var process = new Process
        {
            Name = "Test Process",
            Number = $"P-{Guid.NewGuid():N}",
            ClientId = client.ID,
            LawyerId = lawyer.ID,
            AdversePartName = "Adverse",
            OpposingCounselName = "Opposing",
            Priority = 1,
            CourtInfo = "Court A",
            ProcessTypePhaseId = typePhase.Id,
            ProcessStatusId = pstatus.Id,
            NextHearingDate = DateTime.UtcNow.AddDays(7),
            Description = "A test process"
        };

        // Create
        var created = await repo.Create(process);
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);

        // Get by ID
        var fetched = await repo.GetById(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);

        // Update
        // Clear change tracker to avoid EF Core double-tracking in memory tests
        context.ChangeTracker.Clear();
        fetched.Name = "Updated Name";
        await repo.Update(fetched);

        var updated = await repo.GetById(created.Id);
        Assert.Equal("Updated Name", updated.Name);

        // Delete
        await repo.Delete(created.Id);
        var afterDelete = await repo.GetById(created.Id);
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task GetAll_WithPaginationAndSearch_ReturnsResults()
    {
        await using var context = GetInMemoryDbContext();
        var repo = new ProcessRepository(context);

        var clientUser = new User { ID = Guid.NewGuid(), Name = "Client 1", Email = "c1@test", NIF = "111111111", PasswordHash = "x", IsActive = true };
        var lawyerUser = new User { ID = Guid.NewGuid(), Name = "Lawyer 1", Email = "l1@test", NIF = "222222222", PasswordHash = "x", IsActive = true };
        var client = new Client { ID = clientUser.ID, Address = "somewhere" };
        var lawyer = new Lawyer { ID = lawyerUser.ID, ProfessionalRegister = "ABC-1" };
        await context.Users.AddRangeAsync(clientUser, lawyerUser);
        await context.Clients.AddAsync(client);
        await context.Lawyers.AddAsync(lawyer);
        await context.SaveChangesAsync();

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

        for (int i = 1; i <= 5; i++)
        {
            var process = new Process
            {
                Name = $"Process {i}",
                Number = $"P-{i}",
                ClientId = client.ID,
                LawyerId = lawyer.ID,
                Priority = 1,
                ProcessTypePhaseId = typePhase.Id,
                CourtInfo = "Court Test",
                ProcessStatusId = pstatus.Id
            };
            await repo.Create(process);
        }

        var (list, total) = await repo.GetAll("Process", 1, 2, "name", "asc");
        Assert.Equal(2, list.Count);
        Assert.Equal(5, total);
    }
}
