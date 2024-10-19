using Api.Data.Implementations;
using Api.Data.Interface;
using Api.Models;
using Api.Services.Implementations;
using Api.Services.Interfaces;
using AspNetCore.Identity.Extensions;
using Azure.Identity;
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
    .Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>() // Add role support
    .AddEntityFrameworkStores<PetMinderDbContext>() // Use your existing context
    .AddSignInManager() // Add the SignInManager service
    .AddDefaultTokenProviders();

// Add the built-in Identity API Endpoints (registration, login, etc.)
builder.Services.AddIdentityApiEndpoints<User>();

// Remove manual Authentication configuration since it's already handled by AddIdentityApiEndpoints

// Add Authorization services
builder.Services.AddAuthorization();

// Add Swagger services for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

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

Console.WriteLine($"Current Environment: {app.Environment.EnvironmentName}");

// Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Enable Swagger
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "swagger"; // Serve Swagger at /swagger/index.html
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Ensure Authentication and Authorization are used
app.UseAuthentication(); // Add authentication middleware
app.UseRouting();
app.UseAuthorization(); // Add authorization middleware

// Map controllers
app.MapControllers();
app.MapIdentityApi<User>();

// Apply database migrations (if you have a method for that)
app.ApplyMigrations();

app.Run();
