using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class IndustryConfiguration : IEntityTypeConfiguration<Industry>
	{
		public void Configure(EntityTypeBuilder<Industry> entity)
		{
			entity.ToTable("Industries");
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
			entity.HasIndex(e => e.Name).IsUnique(false);
		}
	}
}
