using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Consilium.Infrastructure.Data;
using Consilium.Application.Interfaces;
using Consilium.Infrastructure.Repositories;
using Consilium.Infrastructure.Services;
using Consilium.API.Endpoints;
using Consilium.Domain.Enums;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// --- Configure Npgsql to handle PostgreSQL enums ---
NpgsqlConnection.GlobalTypeMapper.EnableUnmappedTypes();

// --- Add DB Connection ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- Configure JSON serialization for enums as strings ---
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// --- Register Services (Dependency Injection) ---
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// --- Add API Services ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// --- Configure HTTP Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// --- Map API Endpoints ---
app.MapUserEndpoints();
app.MapClientEndpoints();

app.Run();