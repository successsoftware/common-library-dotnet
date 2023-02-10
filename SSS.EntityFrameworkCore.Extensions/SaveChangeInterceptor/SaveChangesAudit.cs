using SSS.EntityFrameworkCore.Extensions.Entities;
using System;
using System.Collections.Generic;

namespace SSS.EntityFrameworkCore.Extensions
{
    public class SaveChangesAudit : IBaseEntity
    {
        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Succeeded { get; set; }
        public string ErrorMessage { get; set; }
        public ICollection<EntityAudit> EntityAudits { get; set; }
    }

    public class SaveChangesAuditConfiguration : BaseConfiguration<SaveChangesAudit>
    {
    }
}