using Api.Data.Implementations;
using Api.Data.Interface;
using Api.Models;
using Api.Services.Implementations;
using Api.Services.Interfaces;
using AspNetCore.Identity.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Load environment-specific appsettings.{Environment}.json file (e.g., appsettings.Development.json)
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.json",
    optional: true,
    reloadOnChange: true
);

// Add Azure Key Vault to configuration (Key Vault URI from appsettings.json)
var keyVaultUri = builder.Configuration["KeyVault:Uri"];

// fiigure out how to add this to the configuration to ALSO work with docker.

// builder.Configuration.AddAzureKeyVault(
//     new Uri(keyVaultUri),
//     new DefaultAzureCredential(),
//     new Azure.Extensions.AspNetCore.Configuration.Secrets.AzureKeyVaultConfigurationOptions()
// );

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    // Configure Bearer token authentication for Swagger
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token in the following format: Bearer {token}",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        }
    );

    // Add security requirement to enforce using Bearer token
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                new string[] { }
            },
        }
    );
});

// Register Identity and configure database context for Identity
builder.Services.AddDbContext<PetMinderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add IdentityCore services
builder
    .Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<PetMinderDbContext>() // Use your DbContext for Identity
    .AddApiEndpoints(); // Adds the API endpoints for register, login, etc.

// Configure Authentication and Authorization
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme; // Use Bearer scheme by default
        options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
    })
    .AddBearerToken(IdentityConstants.BearerScheme);

builder
    .Services.AddAuthorizationBuilder()
    .AddPolicy(
        "api",
        p =>
        {
            p.RequireAuthenticatedUser();
            p.AddAuthenticationSchemes(IdentityConstants.BearerScheme);
        }
    );

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

// Build the applicationa
var app = builder.Build();

// Enable Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API v1");
    options.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
});

// Enable HTTPS redirection (optional, based on your use case)
// if (!app.Environment.IsDevelopment())
// {
//     app.UseHttpsRedirection(); // Only force HTTPS redirection in non-development environments
// }

// Ensure Authentication and Authorization are used
app.UseAuthentication(); // Add authentication middleware
app.UseRouting();
app.UseAuthorization(); // Add authorization middleware

// Map Identity API endpoints and allow anonymous access
app.MapGroup("api/auth").MapIdentityApi<User>().AllowAnonymous(); // Map Identity API for login, register, etc.

// Map controllers
app.MapControllers(); // Require authorization for all controllers

// Apply database migrations (if you have a method for that)
app.ApplyMigrations();

app.Run();