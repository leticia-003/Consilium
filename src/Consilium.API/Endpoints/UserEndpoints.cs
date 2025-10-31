using Consilium.Application.Interfaces;
using Consilium.API.Dtos;
using Consilium.Domain.Enums;

namespace Consilium.API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users")
            .WithName("Users")
            .WithOpenApi();

        group.MapGet("/", GetAllUsers)
            .WithName("GetAllUsers")
            .WithDescription("Retrieve all users");

        group.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .WithDescription("Retrieve a user by ID");
    }

    private static async Task<IResult> GetAllUsers(IUserRepository repo)
    {
        var users = await repo.GetAll();
        var response = users.Select(u => new UserResponse(
            Id: u.ID,
            Email: u.Email,
            Name: u.Name,
            Phone: u.Phone,
            // Convert string from DB to enum for response
            Status: Enum.TryParse<UserStatus>(u.Status, true, out var status) ? status : UserStatus.INACTIVE
        ));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetUserById(Guid id, IUserRepository repo)
    {
        var user = await repo.GetById(id);

        if (user is null)
            return Results.NotFound(new { message = $"User with ID {id} not found" });

        var response = new UserResponse(
            Id: user.ID,
            Email: user.Email,
            Name: user.Name,
            Phone: user.Phone,
            // Convert string from DB to enum for response
            Status: Enum.TryParse<UserStatus>(user.Status, true, out var status) ? status : UserStatus.INACTIVE
        );

        return Results.Ok(response);
    }
}
