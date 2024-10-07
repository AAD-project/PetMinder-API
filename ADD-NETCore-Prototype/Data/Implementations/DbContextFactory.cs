using Api.Data.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Data.Implementations
{
    public class DbContextFactory : IDbContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DbContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public PetMinderDbContext CreateDbContext()
        {
            // Create a new scope and resolve the DbContext from it
            var scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<PetMinderDbContext>()
                ?? throw new Exception("Could not create DbContext.");
        }
    }
}
