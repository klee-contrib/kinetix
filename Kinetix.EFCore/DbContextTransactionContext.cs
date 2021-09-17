using Kinetix.Services;
using Microsoft.EntityFrameworkCore;

namespace Kinetix.EFCore
{
    internal class DbContextTransactionContext : ITransactionContext
    {
        private readonly DbContext _dbContext;
        private bool _ok = false;

        public DbContextTransactionContext(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbContext.Database.BeginTransaction();
        }

        /// <inheritdoc cref="ITransactionContext.Complete" />
        public void Complete()
        {
            _ok = true;
        }

        /// <inheritdoc cref="ITransactionContext.OnAfterCommit" />
        public void OnAfterCommit()
        {
        }

        /// <inheritdoc cref="ITransactionContext.OnBeforeCommit" />
        public void OnBeforeCommit()
        {
        }

        /// <inheritdoc cref="ITransactionContext.OnCommit" />
        public void OnCommit()
        {
            if (_ok)
            {
                _dbContext.Database.CommitTransaction();
            }
            else
            {
                _dbContext.Database.RollbackTransaction();
            }
        }
    }
}
