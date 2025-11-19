using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Consilium.Domain.Models;
using Consilium.Infrastructure.Data;
using Consilium.API.Dtos;

namespace Consilium.Tests.Endpoints;

public class ProcessEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProcessEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                // Remove existing AppDbContext and related DbContextOptions registrations
                var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) || d.ServiceType == typeof(AppDbContext)).ToList();
                foreach (var d in descriptors)
                {
                    services.Remove(d);
                }

                // Setup InMemory DB
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                });
            });
        });
    }

    private async Task SeedMinimalData(AppDbContext db)
    {
        // Add Users (Client and Lawyer)
        var clientUser = new User { ID = Guid.NewGuid(), Name = "Client Test", Email = "client@test", NIF = "111111111", PasswordHash = "x", IsActive = true };
        var lawyerUser = new User { ID = Guid.NewGuid(), Name = "Lawyer Test", Email = "lawyer@test", NIF = "222222222", PasswordHash = "x", IsActive = true };

        var client = new Client { ID = clientUser.ID, Address = "somewhere" };
        var lawyer = new Lawyer { ID = lawyerUser.ID, ProfessionalRegister = "ABC-1" };

        await db.Users.AddRangeAsync(clientUser, lawyerUser);
        await db.Clients.AddAsync(client);
        await db.Lawyers.AddAsync(lawyer);

        // Add process types/phases/status
        var ptype = new ProcessType { Name = "Civil" };
        var pphase = new ProcessPhase { Name = "Initial" };
        var pstatus = new ProcessStatus { Name = "Open", IsDefault = true};
        await db.ProcessTypes.AddAsync(ptype);
        await db.ProcessPhases.AddAsync(pphase);
        await db.ProcessStatuses.AddAsync(pstatus);
        await db.SaveChangesAsync();

        var typePhase = new ProcessTypePhase { ProcessPhaseId = pphase.Id, ProcessTypeId = ptype.Id, TypePhaseOrder = 1 };
        await db.ProcessTypePhases.AddAsync(typePhase);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Create_Get_Update_Delete_ProcessFlow()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Get DB from the factory and seed
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedMinimalData(db);
        }

        string processNumber = $"P-{Guid.NewGuid():N}";

        // Prepare request
        Guid clientId;
        Guid lawyerId;
        int processTypePhaseId;
        int processStatusId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            clientId = db.Clients.First().ID;
            lawyerId = db.Lawyers.First().ID;
            processTypePhaseId = db.ProcessTypePhases.First().Id;
            processStatusId = db.ProcessStatuses.First().Id;
        }

        var createReq = new CreateProcessRequest(
            Name: "Test Process",
            Number: processNumber,
            ClientId: clientId,
            LawyerId: lawyerId,
            AdversePartName: "Adverse",
            OpposingCounselName: "Opposing",
            Priority: 1,
            CourtInfo: "Court A",
            ProcessTypePhaseId: processTypePhaseId,
            ProcessStatusId: processStatusId,
            NextHearingDate: DateTime.UtcNow.AddDays(7),
            Description: "A test process"
        );

        // Act: Create
        var createResp = await client.PostAsJsonAsync("/api/processes/", createReq);

        // Assert Create
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<ProcessResponse>();
        Assert.NotNull(created);
        Assert.Equal(createReq.Number, created!.Number);

        var processId = created.ProcessId;

        // Act: Get By Id
        var getResp = await client.GetAsync($"/api/processes/{processId}");
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        var fetched = await getResp.Content.ReadFromJsonAsync<ProcessResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(processId, fetched!.ProcessId);

        // Act: Update
        var updateReq = new UpdateProcessRequest(Name: "Updated Name", Number: null, ClientId: null, LawyerId: null, AdversePartName: null, OpposingCounselName: null, Priority: null, CourtInfo: null, ProcessTypePhaseId: null, ProcessStatusId: null, NextHearingDate: null, Description: null, ClosedAt: null);
        var patchResp = await client.PatchAsJsonAsync($"/api/processes/{processId}", updateReq);
        Assert.Equal(HttpStatusCode.OK, patchResp.StatusCode);
        var updated = await patchResp.Content.ReadFromJsonAsync<ProcessResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated!.Name);

        // Act: Delete
        var deleteResp = await client.DeleteAsync($"/api/processes/{processId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        // Act: Get after delete
        var getAfterDel = await client.GetAsync($"/api/processes/{processId}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDel.StatusCode);
    }
}
