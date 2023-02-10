using System;

namespace SSS.EntityFrameworkCore.Extensions.Entities
{
    public class AuditEntity<TKey> : IAuditEntity<TKey>
    {
        public TKey Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    public class AuditEntity : IAuditEntity
    {
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}