using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSS.EntityFrameworkCore.Extensions.SaveChangeInterceptor
{
    public class AuditingInterceptor : ISaveChangesInterceptor
    {
        private SaveChangesAudit _audit;

        public async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var saveChangesAudits = eventData.Context.Set<SaveChangesAudit>();

            _audit = CreateAudit(eventData.Context);

            saveChangesAudits.Add(_audit);

            return await Task.FromResult(result);
        }

        public InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            _audit = CreateAudit(eventData.Context);

            var saveChangesAudits = eventData.Context.Set<SaveChangesAudit>();

            saveChangesAudits.Add(_audit);

            return result;
        }

        public int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            eventData.Context.Attach(_audit);
            _audit.Succeeded = true;
            _audit.EndTime = DateTime.UtcNow;

            return result;
        }

        public async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            eventData.Context.Attach(_audit);
            _audit.Succeeded = true;
            _audit.EndTime = DateTime.UtcNow;

            return await Task.FromResult(result);
        }

        public void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            eventData.Context.Attach(_audit);
            _audit.Succeeded = false;
            _audit.EndTime = DateTime.UtcNow;
            _audit.ErrorMessage = eventData.Exception.Message;
        }

        public async Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            eventData.Context.Attach(_audit);
            _audit.Succeeded = false;
            _audit.EndTime = DateTime.UtcNow;
            _audit.ErrorMessage = eventData.Exception.InnerException?.Message;

            await Task.CompletedTask;
        }

        private static SaveChangesAudit CreateAudit(DbContext context)
        {
            context.ChangeTracker.DetectChanges();

            var audit = new SaveChangesAudit { StartTime = DateTime.UtcNow, EntityAudits = new List<EntityAudit>() };

            foreach (var entry in context.ChangeTracker.Entries())
            {
                var auditMessage = entry.State switch
                {
                    EntityState.Deleted => CreateDeletedMessage(entry),
                    EntityState.Modified => CreateModifiedMessage(entry),
                    EntityState.Added => CreateAddedMessage(entry),
                    _ => null
                };

                if (auditMessage != null)
                {
                    audit.EntityAudits.Add(new EntityAudit { State = entry.State, AuditMessage = auditMessage });
                }
            }

            return audit;

            string CreateAddedMessage(EntityEntry entry)
                => entry.Properties.Aggregate(
                    $"Inserting {entry.Metadata.DisplayName()} with ",
                    (auditString, property) => auditString + $"{property.Metadata.Name}: '{property.CurrentValue}' ");

            string CreateModifiedMessage(EntityEntry entry)
                => entry.Properties.Where(property => property.IsModified || property.Metadata.IsPrimaryKey()).Aggregate(
                    $"Updating {entry.Metadata.DisplayName()} with ",
                    (auditString, property) => auditString + $"{property.Metadata.Name}: '{property.CurrentValue}' ");

            string CreateDeletedMessage(EntityEntry entry)
                => entry.Properties.Where(property => property.Metadata.IsPrimaryKey()).Aggregate(
                    $"Deleting {entry.Metadata.DisplayName()} with ",
                    (auditString, property) => auditString + $"{property.Metadata.Name}: '{property.CurrentValue}' ");
        }
    }
}