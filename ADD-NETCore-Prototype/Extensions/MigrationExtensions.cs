using Api.Data.Implementations;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Identity.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using PetMinderDbContext context =
            scope.ServiceProvider.GetRequiredService<PetMinderDbContext>();

        context.Database.Migrate();
    }
}
