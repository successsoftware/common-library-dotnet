using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSS.EntityFrameworkCore.Extensions
{
    public interface IDbContext
    {
        DatabaseFacade Database { get; }
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        int SaveChanges();
        int SaveChanges(bool acceptAllChangesOnSuccess = true);
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);
        Task CommitAsync(bool isAudit = true);
        Task CommitAsync(Func<Task> action);
    }
}
