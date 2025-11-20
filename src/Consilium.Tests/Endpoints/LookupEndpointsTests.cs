using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Consilium.Infrastructure.Data;
using Consilium.Domain.Models;
using Consilium.API.Dtos;

namespace Consilium.Tests.Endpoints;

public class LookupEndpointsTests
{
    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
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
            });
        });
    }

    private async Task SeedLookups(AppDbContext db)
    {
        // Clean existing data to avoid contamination from other tests
        db.ProcessTypePhases.RemoveRange(db.ProcessTypePhases);
        db.ProcessTypes.RemoveRange(db.ProcessTypes);
        db.ProcessPhases.RemoveRange(db.ProcessPhases);
        db.ProcessStatuses.RemoveRange(db.ProcessStatuses);
        await db.SaveChangesAsync();

        var t1 = new ProcessType { Name = "Type A", IsActive = true };
        var t2 = new ProcessType { Name = "Type B", IsActive = false };
        await db.ProcessTypes.AddRangeAsync(t1, t2);

        var p1 = new ProcessPhase { Name = "Phase 1", IsActive = true };
        var p2 = new ProcessPhase { Name = "Phase 2", IsActive = false };
        await db.ProcessPhases.AddRangeAsync(p1, p2);

        var s1 = new ProcessStatus { Name = "Open", IsActive = true, IsDefault = true, IsFinal = false };
        var s2 = new ProcessStatus { Name = "Closed", IsActive = false, IsDefault = false, IsFinal = true };
        await db.ProcessStatuses.AddRangeAsync(s1, s2);

        await db.SaveChangesAsync();

        var ptp = new ProcessTypePhase { ProcessTypeId = t1.Id, ProcessPhaseId = p1.Id, TypePhaseOrder = 1, IsActive = true };
        await db.ProcessTypePhases.AddAsync(ptp);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetProcessTypes_ReturnsOnlyActive()
    {
        using var factory = CreateFactory();
        var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedLookups(db);
        }

        var resp = await client.GetAsync("/api/lookups/process-types");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<List<ProcessTypeResponse>>();
        Assert.NotNull(json);
        Assert.Single(json!);
        Assert.Equal("Type A", json!.First().Name);
    }

    [Fact]
    public async Task GetProcessPhases_ReturnsOnlyActive()
    {
        using var factory = CreateFactory();
        var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedLookups(db);
        }

        var resp = await client.GetAsync("/api/lookups/process-phases");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<List<ProcessPhaseResponse>>();
        Assert.NotNull(json);
        Assert.Single(json!);
        Assert.Equal("Phase 1", json!.First().Name);
    }

    [Fact]
    public async Task GetProcessStatuses_ReturnsOnlyActive()
    {
        using var factory = CreateFactory();
        var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedLookups(db);
        }

        var resp = await client.GetAsync("/api/lookups/process-statuses");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<List<ProcessStatusResponse>>();
        Assert.NotNull(json);
        Assert.Single(json!);
        Assert.Equal("Open", json!.First().Name);
    }

    [Fact]
    public async Task GetProcessTypePhases_ReturnsMapping()
    {
        using var factory = CreateFactory();
        var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedLookups(db);
        }

        var resp = await client.GetAsync("/api/lookups/process-type-phases");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<List<ProcessTypePhaseResponse>>();
        Assert.NotNull(json);
        Assert.Single(json!);
        Assert.Equal(1, json!.First().Order);
    }

    [Fact]
    public async Task GetPhasesForProcessType_ReturnsOrderedPhases()
    {
        using var factory = CreateFactory();
        var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedLookups(db);
            // Add a second phase for same type with order 2
            var type = db.ProcessTypes.First();
            var phase = new ProcessPhase { Name = "Phase 3", IsActive = true };
            await db.ProcessPhases.AddAsync(phase);
            await db.SaveChangesAsync();
            var ptp2 = new ProcessTypePhase { ProcessTypeId = type.Id, ProcessPhaseId = phase.Id, TypePhaseOrder = 2, IsActive = true };
            await db.ProcessTypePhases.AddAsync(ptp2);
            await db.SaveChangesAsync();
        }

        int typeId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            typeId = db.ProcessTypes.First().Id;
        }

        var resp = await client.GetAsync($"/api/lookups/process-types/{typeId}/phases");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<List<ProcessPhaseResponse>>();
        Assert.NotNull(json);
        Assert.True(json!.Count >= 2);
        Assert.Equal("Phase 1", json!.First().Name);
    }
    [Fact]
    public async Task GetProcessPhases_ReturnsOnlyActive()
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
            });
        });
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedLookups(db);
        }

        var client = factory.CreateClient();

        var resp = await client.GetAsync("/api/lookups/process-phases");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<List<ProcessPhaseResponse>>();
        Assert.NotNull(json);
        Assert.Single(json!);
        Assert.Equal("Phase 1", json!.First().Name);
    }

    [Fact]
    public async Task GetProcessStatuses_ReturnsOnlyActive()
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
            });
        });
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedLookups(db);
        }

        var client = factory.CreateClient();

        var resp = await client.GetAsync("/api/lookups/process-statuses");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<List<ProcessStatusResponse>>();
        Assert.NotNull(json);
        Assert.Single(json!);
        Assert.Equal("Open", json!.First().Name);
    }

    [Fact]
    public async Task GetProcessTypePhases_ReturnsMapping()
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
            });
        });
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedLookups(db);
        }

        var client = factory.CreateClient();

        var resp = await client.GetAsync("/api/lookups/process-type-phases");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<List<ProcessTypePhaseResponse>>();
        Assert.NotNull(json);
        Assert.Single(json!);
        Assert.Equal(1, json!.First().Order);
    }

    [Fact]
    public async Task GetPhasesForProcessType_ReturnsOrderedPhases()
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
            });
        });
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedLookups(db);
            // Add a second phase for same type with order 2
            var type = db.ProcessTypes.First();
            var phase = new ProcessPhase { Name = "Phase 3", IsActive = true };
            await db.ProcessPhases.AddAsync(phase);
            await db.SaveChangesAsync();
            var ptp2 = new ProcessTypePhase { ProcessTypeId = type.Id, ProcessPhaseId = phase.Id, TypePhaseOrder = 2, IsActive = true };
            await db.ProcessTypePhases.AddAsync(ptp2);
            await db.SaveChangesAsync();
        }

        var typeId = 0;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            typeId = db.ProcessTypes.First().Id;
        }

        var client = _factory.CreateClient();

        var resp = await client.GetAsync($"/api/lookups/process-types/{typeId}/phases");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<List<ProcessPhaseResponse>>();
        Assert.NotNull(json);
        Assert.True(json!.Count >= 2);
        Assert.Equal("Phase 1", json!.First().Name);
    }
}
