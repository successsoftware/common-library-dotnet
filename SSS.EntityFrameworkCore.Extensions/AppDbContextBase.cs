using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SSS.CommonLib.Interfaces;
using SSS.EntityFrameworkCore.Extensions.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SSS.EntityFrameworkCore.Extensions
{
    public abstract class AppDbContextBase : DbContext, IDbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDateTimeService _dateTimeService;

        protected AppDbContextBase(DbContextOptions options) : base(options) { }

        protected AppDbContextBase(DbContextOptions options, IHttpContextAccessor httpContextAccessor,
            IDateTimeService dateTimeService) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _dateTimeService = dateTimeService;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ConfigureEntities(builder);
            base.OnModelCreating(builder);
        }

        protected virtual void ConfigureEntities(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override int SaveChanges()
        {
            BeforeCommit();

            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess = true)
        {
            BeforeCommit();

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
        {
            BeforeCommit();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            BeforeCommit();

            return base.SaveChangesAsync(cancellationToken);
        }

        public async Task CommitAsync(bool isAudit = true)
        {
            if (isAudit) BeforeCommit();

            await base.SaveChangesAsync();
        }

        public async Task CommitAsync(Func<Task> action)
        {
            BeforeCommit();

            var strategy = Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = Database.BeginTransaction();

                try
                {
                    await base.SaveChangesAsync();

                    await action();
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message, ex);

                    transaction.Rollback();

                    throw;
                }
                finally
                {
                    transaction.Commit();
                }
            });
        }

        private void BeforeCommit()
        {
            var entriesAdded = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity);

            var entriesModified = ChangeTracker.Entries()
                  .Where(e => e.State == EntityState.Modified).Select(e => e.Entity as IAuditEntity);

            if (entriesAdded.Any()) ProcessAudit(entriesAdded, EntityState.Added);

            if (entriesModified.Any()) ProcessAudit(entriesModified, EntityState.Modified);
        }

        private void ProcessAudit(IEnumerable<object> entries, EntityState state)
        {
            var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            foreach (var e in entries.Select(e => e as IAuditEntity))
            {
                if (e is not null)
                {
                    if (state == EntityState.Added)
                    {
                        e.CreatedBy = userId;
                        e.CreatedAt = _dateTimeService.UtcNow;
                    }
                    else
                    {
                        e.ModifiedBy = userId;
                        e.ModifiedAt = _dateTimeService.UtcNow;
                    }
                }
            }
        }
    }
}
