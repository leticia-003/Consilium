using Consilium.Application.Interfaces;
using Consilium.Domain.Enums;
using Consilium.API.Dtos;
using Consilium.API.Services;
using Microsoft.EntityFrameworkCore;

namespace Consilium.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithName("Authentication")
            .WithOpenApi();

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithDescription("Authenticate user and return JWT token")
            .AllowAnonymous();
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IUserRepository userRepo,
        IPasswordHasher hasher,
        JwtTokenService tokenService)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
            return Results.BadRequest(new { message = "Email is required" });

        if (string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest(new { message = "Password is required" });

        // Find user by email
        var users = await userRepo.GetAll();
        var user = users.FirstOrDefault(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

        if (user is null)
            return Results.Unauthorized();

        // Verify password
        if (!hasher.VerifyPassword(request.Password, user.PasswordHash))
            return Results.Unauthorized();

        // Check if user is active
        if (!user.IsActive)
            return Results.BadRequest(new { message = "User account is inactive" });

        // Generate JWT token
        var token = tokenService.GenerateToken(user);

        // Determine status based on IsActive
        var status = user.IsActive ? UserStatus.ACTIVE : UserStatus.INACTIVE;

        var response = new LoginResponse(
            Token: token,
            UserId: user.ID,
            Email: user.Email,
            Name: user.Name,
            Status: status
        );

        return Results.Ok(response);
    }
}
