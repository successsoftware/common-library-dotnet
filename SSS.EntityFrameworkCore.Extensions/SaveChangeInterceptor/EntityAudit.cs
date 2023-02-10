using Microsoft.EntityFrameworkCore;
using SSS.EntityFrameworkCore.Extensions.Entities;

namespace SSS.EntityFrameworkCore.Extensions
{
    public class EntityAudit : IBaseEntity
    {
        public string Id { get; set; }
        public EntityState State { get; set; }
        public string AuditMessage { get; set; }

        public SaveChangesAudit SaveChangesAudit { get; set; }
    }

    public class EntityAuditConfiguration : BaseConfiguration<EntityAudit>
    {
    }
}