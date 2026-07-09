using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessIdea.Domain.Common;
using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BusinessIdea.Infrastructure.Persistence.Configurations
{
    public abstract class BaseEntityConfiguration<TEntity>
    : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasQueryFilter(x => x.DeletedOn == null);
    }
}
}