using System;
using Api.Data.Implementations;
using Api.Data.Interface;

namespace Api.Services.Implementations
{
    public abstract class BaseService
    {
        private readonly IDbContextFactory _dbContextFactory;

        protected BaseService(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        protected Task<PetMinderDbContext> CreateDbContextAsync()
        {
            return Task.FromResult(_dbContextFactory.CreateDbContext())
                ?? throw new Exception("Could not create DbContext.");
        }
    }
}
