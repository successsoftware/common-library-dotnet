using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SSS.CommonLib.Interfaces;

namespace SSS.EntityFrameworkCore.Extensions
{
    public class AuditContext : AppDbContextBase
    {
        public AuditContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor,
            IDateTimeService dateTimeService) : base(options, httpContextAccessor, dateTimeService)
        {
        }

        public DbSet<SaveChangesAudit> SaveChangesAudits { get; set; }

        public DbSet<EntityAudit> EntityAudits { get; set; }
    }
}