using System;

namespace SSS.EntityFrameworkCore.Extensions.Entities
{
    public interface IAuditEntity<TKey> : IBaseEntity<TKey>
    {
        string CreatedBy { get; set; }
        DateTime CreatedAt { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedAt { get; set; }
    }

    public interface IAuditEntity : IBaseEntity
    {
        string CreatedBy { get; set; }
        DateTime CreatedAt { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedAt { get; set; }
    }
}