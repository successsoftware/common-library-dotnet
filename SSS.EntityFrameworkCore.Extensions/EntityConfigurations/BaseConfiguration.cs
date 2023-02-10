using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSS.EntityFrameworkCore.Extensions.Entities;

namespace SSS.EntityFrameworkCore.Extensions
{
    public abstract class BaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
            where TEntity : class, IBaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
        }
    }
}
