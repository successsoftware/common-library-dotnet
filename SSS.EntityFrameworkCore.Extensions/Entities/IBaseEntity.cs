using System.ComponentModel.DataAnnotations;

namespace SSS.EntityFrameworkCore.Extensions.Entities
{
    public interface IBaseEntity<TKey>
    {
        [Key]
        TKey Id { get; set; }
    }

    public interface IBaseEntity : IBaseEntity<string> { }
}
