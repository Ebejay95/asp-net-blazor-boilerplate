using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
	public class TagConfiguration : IEntityTypeConfiguration<Tag>
	{
		public void Configure(EntityTypeBuilder<Tag> entity)
		{
			entity.ToTable("Tags");
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
			entity.HasIndex(e => e.Name).IsUnique(false);
		}
	}
}
