using Microsoft.EntityFrameworkCore;
using SSS.EntityFrameworkCore.Extensions.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SSS.EntityFrameworkCore.Extensions
{
    public static class DbContextExtension
    {
        public static void PrepareAuditInfo(this DbContext dbContext, string userId, DateTime utcNow)
        {
            var entriesAdded = dbContext.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity);

            var entriesModified = dbContext.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified).Select(e => e.Entity as IAuditEntity);

            if (entriesAdded.Any())
                ProcessAudit(entriesAdded, EntityState.Added, userId, utcNow);

            if (entriesModified.Any())
                ProcessAudit(entriesModified, EntityState.Modified, userId, utcNow);
        }

        private static void ProcessAudit(IEnumerable<object> entries, EntityState state, string userId, DateTime utcNow)
        {
            foreach (var e in entries.Select(e => e as IAuditEntity))
            {
                if (e is null) continue;

                if (state == EntityState.Added)
                {
                    e.CreatedBy = userId;
                    e.CreatedAt = utcNow;
                }
                else
                {
                    e.ModifiedBy = userId;
                    e.ModifiedAt = utcNow;
                }
            }
        }
    }
}