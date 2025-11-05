using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Consilium.Domain.Models;
using Consilium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Consilium.API.Services;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public JwtTokenService(IConfiguration configuration, AppDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    /// <summary>
    /// Determines the role of a user based on their associated entities (Client, Lawyer, Admin)
    /// </summary>
    private async Task<string> DetermineUserRole(Guid userId)
    {
        // Check if user is Admin
        var isAdmin = await _context.Admins.AnyAsync(a => a.ID == userId);
        if (isAdmin)
            return "Admin";

        // Check if user is Lawyer
        var isLawyer = await _context.Lawyers.AnyAsync(l => l.ID == userId);
        if (isLawyer)
            return "Lawyer";

        // Check if user is Client
        var isClient = await _context.Clients.AnyAsync(c => c.ID == userId);
        if (isClient)
            return "Client";

        // Default role if none match
        return "User";
    }

    public async Task<string> GenerateToken(User user)
    {
        // Get JWT settings from configuration
        var secretKey = _configuration["Jwt:Key"] ?? "ConsiliumSecretKeyForDevelopment_MustBeAtLeast32CharactersLong";
        var issuer = _configuration["Jwt:Issuer"] ?? "https://localhost:8080";
        var audience = _configuration["Jwt:Audience"] ?? "http://localhost:4200";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Determine user role
        var role = await DetermineUserRole(user.ID);

        // Create claims with the exact structure requested
        var claims = new[]
        {
            new Claim("user_id", user.ID.ToString()),
            new Claim("username", user.Name),
            new Claim("role", role),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("nif", user.NIF),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Token expiration: 8 hours from now
        var expirationTime = DateTime.UtcNow.AddHours(8);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expirationTime,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
