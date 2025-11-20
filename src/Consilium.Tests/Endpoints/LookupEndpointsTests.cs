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
                    options.UseInMemoryDatabase($"LookupTestDB_{Guid.NewGuid()}");
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                });
            });
        });
    }

    private async Task SeedLookups(AppDbContext db)
    {
        // Clean existing data to avoid contamination from other tests
        if (db.ProcessTypePhases.Any())
        {
            db.ProcessTypePhases.RemoveRange(db.ProcessTypePhases);
        }
        if (db.ProcessTypes.Any())
        {
            db.ProcessTypes.RemoveRange(db.ProcessTypes);
        }
        if (db.ProcessPhases.Any())
        {
            db.ProcessPhases.RemoveRange(db.ProcessPhases);
        }
        if (db.ProcessStatuses.Any())
        {
            db.ProcessStatuses.RemoveRange(db.ProcessStatuses);
        }
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










}