using Api.Data.Implementations;
using Api.Data.Interface;
using Api.Data.Seeders;
using Api.Models;
using Api.Services.Implementations;
using Api.Services.Interfaces;
using AspNetCore.Identity.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Keep existing service registrations as is
builder.Services.AddSingleton<IPetService, PetService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<ITodoTaskService, TodoTaskService>();
builder.Services.AddSingleton<IReminderService, ReminderService>();
builder.Services.AddSingleton<IDbContextFactory, DbContextFactory>();
builder.Services.AddScoped<DatabaseSeeder>();

// Add controllers
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Authentication and Authorization
builder.Services.AddAuthorization();

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
    .AddBearerToken(IdentityConstants.BearerScheme);

builder
    .Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<PetMinderDbContext>()
    .AddApiEndpoints();

// Register the DbContextFactory correctly
builder.Services.AddDbContextFactory<PetMinderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Show detailed errors in development
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use HTTPS redirection
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map controllers and explicitly allow anonymous access where needed
app.MapControllers();

// Apply database migrations
app.ApplyMigrations();

// Map identity endpoints, keeping in mind to support API access
app.MapIdentityApi<User>().AllowAnonymous(); //sus out the AllowAnonymous

// Run the application
app.Run();
