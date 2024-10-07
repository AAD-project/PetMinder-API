using Api.Data.Implementations;

namespace Api.Data.Interface
{
    public interface IDbContextFactory
    {
        PetMinderDbContext CreateDbContext();
    }
}
