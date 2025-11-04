using Consilium.Application.Interfaces;
using Consilium.Domain.Models;
using Consilium.Domain.Enums;
using Consilium.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Consilium.API.Endpoints;

public static class ClientEndpoints
{
    public static void MapClientEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/clients")
            .WithName("Clients")
            .WithOpenApi();

        group.MapGet("/", GetAllClients)
            .WithName("GetAllClients")
            .WithDescription("Retrieve all clients");

        group.MapGet("/{id:guid}", GetClientById)
            .WithName("GetClientById")
            .WithDescription("Retrieve a client by ID");

        group.MapPost("/", CreateClient)
            .WithName("CreateClient")
            .WithDescription("Create a new client");
    }

    private static async Task<IResult> GetAllClients(
        IClientRepository repo,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortOrder = "asc")
    {
        var (clients, totalCount) = await repo.GetAll(search, status, page, limit, sortBy, sortOrder);

        var response = clients.Select(c => new ClientResponse(
            Id: c.ID,
            Email: c.User?.Email ?? string.Empty,
            Name: c.User?.Name ?? string.Empty,
            Status: c.User?.IsActive == true ? UserStatus.ACTIVE : UserStatus.INACTIVE,
            NIF: c.User?.NIF ?? string.Empty,
            Address: c.Address
        ));

        return Results.Ok(new {
            data = response,
            meta = new {
                totalCount,
                page,
                limit
            }
        });
    }


    private static async Task<IResult> GetClientById(Guid id, IClientRepository repo)
    {
        var client = await repo.GetById(id);
        
        if (client is null)
            return Results.NotFound(new { message = $"Client with ID {id} not found" });

        var response = new ClientResponse(
            Id: client.ID,
            Email: client.User?.Email ?? string.Empty,
            Name: client.User?.Name ?? string.Empty,
            Status: client.User?.IsActive == true ? UserStatus.ACTIVE : UserStatus.INACTIVE,
            NIF: client.User?.NIF ?? string.Empty,
            Address: client.Address
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateClient(
        CreateClientRequest request,
        IClientRepository repo,
        IPasswordHasher hasher)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
            return Results.BadRequest(new { message = "Email is required" });

        if (string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest(new { message = "Password is required" });

        if (string.IsNullOrWhiteSpace(request.NIF) || request.NIF.Length != 9)
            return Results.BadRequest(new { message = "NIF must be a 9-character string" });

        // Hash the password
        var hashedPassword = hasher.HashPassword(request.Password);

        // Create user and client
        var user = new User
        {
            Email = request.Email,
            PasswordHash = hashedPassword,
            Name = request.Name,
            NIF = request.NIF,
            IsActive = true
        };

        var client = new Client
        {
            Address = request.Address ?? string.Empty
        };

        // Save to database
        var newClient = await repo.Create(user, client);

        // Prepare response
        var response = new ClientResponse(
            Id: newClient.ID,
            Email: newClient.User?.Email ?? string.Empty,
            Name: newClient.User?.Name ?? string.Empty,
            Status: UserStatus.ACTIVE,
            NIF: newClient.User?.NIF ?? string.Empty,
            Address: newClient.Address
        );

        return Results.Created($"/api/clients/{newClient.ID}", response);
    }
}
