using Microsoft.EntityFrameworkCore;
using Consilium.Infrastructure.Data;
using Consilium.Application.Interfaces;
using Consilium.Infrastructure.Repositories;
using Consilium.Domain.Models;
using Consilium.Domain.Enums;
using Consilium.API.Dtos;

// --- 2. START YOUR EXECUTABLE CODE AFTER ALL TYPE DEFINITIONS ---
var builder = WebApplication.CreateBuilder(args);

// --- Add DB Connection ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)); // Use value converter for enums in AppDbContext

// --- Register Your Services (Dependency Injection) ---
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer(); // For Swagger
builder.Services.AddSwaggerGen();           // For Swagger
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// --- 3. Create Your API Endpoints ---
app.MapGet("/users", async (IUserRepository repo) => 
    await repo.GetAll());

app.MapGet("/users/{id:guid}", async (Guid id, IUserRepository repo) =>
{
    var user = await repo.GetById(id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
});

app.MapGet("/clients", async (IClientRepository repo) =>
    await repo.GetAll());

app.MapGet("/clients/{id:guid}", async (Guid id, IClientRepository repo) =>
{
    var client = await repo.GetById(id);
    return client is not null ? Results.Ok(client) : Results.NotFound();
});

// The record is already defined at the top, so this works now
app.MapPost("/clients", async (CreateClientRequest request, IClientRepository repo) =>
{
    // TODO: Hash the password properly!
    var user = new User
    {
        Email = request.Email,
        PasswordHash = request.Password, // Never do this in production
        Name = request.Name,
        Phone = request.Phone,
        Status = UserStatus.ACTIVE
    };

    var client = new Client
    {
        NIF = request.NIF,
        Address = request.Address
    };

    var newClient = await repo.Create(user, client);
    return Results.Created($"/clients/{newClient.ID}", newClient);
});

app.Run();