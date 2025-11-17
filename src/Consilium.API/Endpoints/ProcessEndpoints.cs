using Consilium.API.Dtos;
using Consilium.Application.Interfaces;
using Consilium.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Consilium.Infrastructure.Data;

namespace Consilium.API.Endpoints;

public static class ProcessEndpoints
{
    public static void MapProcessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/processes")
            .WithName("Processes")
            .WithOpenApi();

        group.MapGet("/", GetAllProcesses)
            .WithName("GetAllProcesses")
            .WithDescription("Retrieve all processes");

        group.MapGet("/{id:guid}", GetProcessById)
            .WithName("GetProcessById")
            .WithDescription("Retrieve a process by ID");

        group.MapPost("/", CreateProcess)
            .WithName("CreateProcess")
            .WithDescription("Create a new legal process");

        group.MapPatch("/{id:guid}", UpdateProcess)
            .WithName("UpdateProcess")
            .WithDescription("Update process");

        group.MapDelete("/{id:guid}", DeleteProcess)
            .WithName("DeleteProcess")
            .WithDescription("Delete a process");
    }

    private static async Task<IResult> GetAllProcesses(
        IProcessRepository repo,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortOrder = "asc")
    {
        try
        {
            var (processes, totalCount) = await repo.GetAll(search, page, limit, sortBy, sortOrder);

            var response = processes.Select(p => new ProcessResponse(
                ProcessId: p.Id,
                Name: p.Name,
                Number: p.Number,
                ClientId: p.ClientId,
                LawyerId: p.LawyerId,
                AdversePartName: p.AdversePartName,
                OpposingCounselName: p.OpposingCounselName,
                CreatedAt: p.CreatedAt,
                ClosedAt: p.ClosedAt,
                Priority: p.Priority,
                CourtInfo: p.CourtInfo,
                ProcessTypePhaseId: p.ProcessTypePhaseId,
                ProcessStatusId: p.ProcessStatusId,
                Description: p.Description,
                NextHearingDate: p.NextHearingDate
            ));

            return Results.Ok(new { data = response, meta = new { totalCount, page, limit } });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01" || ex.SqlState == "42703")
        {
            // Missing relation/column (DB not seeded) -> return empty list instead of 500
            return Results.Ok(new { data = Array.Empty<object>(), meta = new { totalCount = 0, page, limit } });
        }
    }

    private static async Task<IResult> GetProcessById(Guid id, IProcessRepository repo)
    {
        Process? process;
        try
        {
            process = await repo.GetById(id);
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01" || ex.SqlState == "42703")
        {
            // Missing relation/column (DB not seeded) -> return 404 so client sees an empty result
            return Results.NotFound(new { message = $"Process with ID {id} not found" });
        }

        if (process == null)
            return Results.NotFound(new { message = $"Process with ID {id} not found" });

        var response = new ProcessResponse(
            ProcessId: process.Id,
            Name: process.Name,
            Number: process.Number,
            ClientId: process.ClientId,
            LawyerId: process.LawyerId,
            AdversePartName: process.AdversePartName,
            OpposingCounselName: process.OpposingCounselName,
            CreatedAt: process.CreatedAt,
            ClosedAt: process.ClosedAt,
            Priority: process.Priority,
            CourtInfo: process.CourtInfo,
            ProcessTypePhaseId: process.ProcessTypePhaseId,
            ProcessStatusId: process.ProcessStatusId,
            Description: process.Description,
            NextHearingDate: process.NextHearingDate
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateProcess(
        CreateProcessRequest request,
        IProcessRepository repo,
        IClientRepository clientRepo,
        ILawyerRepository lawyerRepo,
        AppDbContext db)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest(new { message = "Name is required" });

        if (string.IsNullOrWhiteSpace(request.Number))
            return Results.BadRequest(new { message = "Number is required" });

        if (request.Priority < 0)
            return Results.BadRequest(new { message = "Priority must be non-negative" });

        // Validate foreign keys
        var client = await clientRepo.GetById(request.ClientId);
        if (client == null)
            return Results.BadRequest(new { message = $"Client with ID {request.ClientId} not found" });

        var lawyer = await lawyerRepo.GetById(request.LawyerId);
        if (lawyer == null)
            return Results.BadRequest(new { message = $"Lawyer with ID {request.LawyerId} not found" });

        var typePhaseExists = await db.ProcessTypePhases.AnyAsync(x => x.Id == request.ProcessTypePhaseId);
        if (!typePhaseExists)
            return Results.BadRequest(new { message = $"ProcessTypePhase with ID {request.ProcessTypePhaseId} not found" });

        var statusExists = await db.ProcessStatuses.AnyAsync(x => x.Id == request.ProcessStatusId);
        if (!statusExists)
            return Results.BadRequest(new { message = $"ProcessStatus with ID {request.ProcessStatusId} not found" });

        var process = new Process
        {
            Name = request.Name,
            Number = request.Number,
            ClientId = request.ClientId,
            LawyerId = request.LawyerId,
            AdversePartName = request.AdversePartName,
            OpposingCounselName = request.OpposingCounselName,
            Priority = request.Priority,
            CourtInfo = request.CourtInfo,
            ProcessTypePhaseId = request.ProcessTypePhaseId,
            ProcessStatusId = request.ProcessStatusId,
            NextHearingDate = request.NextHearingDate,
            Description = request.Description
        };

        try
        {
            var created = await repo.Create(process);

            var response = new ProcessResponse(
            ProcessId: created.Id,
            Name: created.Name,
            Number: created.Number,
            ClientId: created.ClientId,
            LawyerId: created.LawyerId,
            AdversePartName: created.AdversePartName,
            OpposingCounselName: created.OpposingCounselName,
            CreatedAt: created.CreatedAt,
            ClosedAt: created.ClosedAt,
            Priority: created.Priority,
            CourtInfo: created.CourtInfo,
            ProcessTypePhaseId: created.ProcessTypePhaseId,
            ProcessStatusId: created.ProcessStatusId,
            Description: created.Description,
            NextHearingDate: created.NextHearingDate
        );

            return Results.Created($"/api/processes/{created.Id}", response);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            if (dbEx.InnerException is Npgsql.PostgresException pgEx)
            {
                if (pgEx.SqlState == "23503") // foreign key violation
                    return Results.BadRequest(new { message = "Invalid foreign key (client/lawyer/status/typePhase) provided" });

                if (pgEx.SqlState == "23505") // unique violation
                    return Results.Conflict(new { message = "Process number must be unique for this client and lawyer" });
            }

            throw;
        }
    }

    private static async Task<IResult> UpdateProcess(Guid id, UpdateProcessRequest request, IProcessRepository repo, AppDbContext db, IClientRepository clientRepo, ILawyerRepository lawyerRepo)
    {
        var existing = await repo.GetById(id);
        if (existing == null)
            return Results.NotFound(new { message = $"Process with ID {id} not found" });

        if (!string.IsNullOrWhiteSpace(request.Name))
            existing.Name = request.Name;
        if (!string.IsNullOrWhiteSpace(request.Number))
            existing.Number = request.Number;
        if (request.ClientId.HasValue)
        {
            existing.ClientId = request.ClientId.Value;
            var clientExists = await clientRepo.GetById(request.ClientId.Value);
            if (clientExists == null)
                return Results.BadRequest(new { message = $"Client with ID {request.ClientId.Value} not found" });
        }
        if (request.LawyerId.HasValue)
        {
            existing.LawyerId = request.LawyerId.Value;
            var lawyerExists = await lawyerRepo.GetById(request.LawyerId.Value);
            if (lawyerExists == null)
                return Results.BadRequest(new { message = $"Lawyer with ID {request.LawyerId.Value} not found" });
        }
        if (!string.IsNullOrWhiteSpace(request.AdversePartName))
            existing.AdversePartName = request.AdversePartName;
        if (!string.IsNullOrWhiteSpace(request.OpposingCounselName))
            existing.OpposingCounselName = request.OpposingCounselName;
        if (request.Priority.HasValue)
            existing.Priority = request.Priority.Value;
        if (!string.IsNullOrWhiteSpace(request.CourtInfo))
            existing.CourtInfo = request.CourtInfo;
        if (request.ProcessTypePhaseId.HasValue)
        {
            existing.ProcessTypePhaseId = request.ProcessTypePhaseId.Value;
            var typePhaseExists = await db.ProcessTypePhases.AnyAsync(x => x.Id == request.ProcessTypePhaseId.Value);
            if (!typePhaseExists)
                return Results.BadRequest(new { message = $"ProcessTypePhase with ID {request.ProcessTypePhaseId.Value} not found" });
        }
        if (request.ProcessStatusId.HasValue)
        {
            existing.ProcessStatusId = request.ProcessStatusId.Value;
            var statusExists = await db.ProcessStatuses.AnyAsync(x => x.Id == request.ProcessStatusId.Value);
            if (!statusExists)
                return Results.BadRequest(new { message = $"ProcessStatus with ID {request.ProcessStatusId.Value} not found" });
        }
        if (request.NextHearingDate.HasValue)
            existing.NextHearingDate = request.NextHearingDate;
        if (!string.IsNullOrWhiteSpace(request.Description))
            existing.Description = request.Description;
        if (request.ClosedAt.HasValue)
            existing.ClosedAt = request.ClosedAt;

        await repo.Update(existing);

        var updated = await repo.GetById(id);

        var response = new ProcessResponse(
            ProcessId: updated!.Id,
            Name: updated.Name,
            Number: updated.Number,
            ClientId: updated.ClientId,
            LawyerId: updated.LawyerId,
            AdversePartName: updated.AdversePartName,
            OpposingCounselName: updated.OpposingCounselName,
            CreatedAt: updated.CreatedAt,
            ClosedAt: updated.ClosedAt,
            Priority: updated.Priority,
            CourtInfo: updated.CourtInfo,
            ProcessTypePhaseId: updated.ProcessTypePhaseId,
            ProcessStatusId: updated.ProcessStatusId,
            Description: updated.Description,
            NextHearingDate: updated.NextHearingDate
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteProcess(Guid id, IProcessRepository repo)
    {
        try
        {
            await repo.Delete(id);
            return Results.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound(new { message = $"Process with ID {id} not found" });
        }
    }
}
