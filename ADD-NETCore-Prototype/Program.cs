using Api.Data.Implementations;
using Api.Data.Interface;
using Api.Models;
using Api.Services.Implementations;
using Api.Services.Interfaces;
using AspNetCore.Identity.Extensions;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load environment-specific appsettings.{Environment}.json file (e.g., appsettings.Development.json)
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.json",
    optional: true,
    reloadOnChange: true
);

// Add Azure Key Vault to configuration (Key Vault URI from appsettings.json)
var keyVaultUri = builder.Configuration["KeyVault:Uri"];
builder.Configuration.AddAzureKeyVault(
    new Uri(keyVaultUri),
    new DefaultAzureCredential(),
    new Azure.Extensions.AspNetCore.Configuration.Secrets.AzureKeyVaultConfigurationOptions()
);

// Register Identity and configure database context for Identity
builder.Services.AddDbContext<PetMinderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add IdentityCore services
builder
    .Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<PetMinderDbContext>() // Use your DbContext for Identity
    .AddApiEndpoints(); // Adds the API endpoints for register, login, etc.

// Configure Authentication and Authorization
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    })
    .AddCookie(
        IdentityConstants.ApplicationScheme,
        options =>
        {
            // Custom cookie behavior for unauthenticated or unauthorized access
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        }
    )
    .AddBearerToken(IdentityConstants.BearerScheme); // Add support for token-based authentication

builder.Services.AddAuthorization(); // Add Authorization services

// Add Swagger services for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add other services (as needed)
builder.Services.AddSingleton<IPetService, PetService>();
builder.Services.AddSingleton<ITodoTaskService, TodoTaskService>();
builder.Services.AddSingleton<IReminderService, ReminderService>();
builder.Services.AddSingleton<IDbContextFactory, DbContextFactory>();
builder.Services.AddScoped<IUserService, UserService>();

// Add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add this to register controllers
builder.Services.AddControllers();

// Build the application
var app = builder.Build();

// Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable HTTPS redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Ensure Authentication and Authorization are used
app.UseAuthentication(); // Add authentication middleware
app.UseRouting();
app.UseAuthorization(); // Add authorization middleware

// Map Identity API endpoints and allow anonymous access
app.MapIdentityApi<User>().AllowAnonymous(); // Map Identity API for login, register, etc.

// Map controllers
app.MapControllers();

// Apply database migrations (if you have a method for that)
app.ApplyMigrations();

app.Run();
