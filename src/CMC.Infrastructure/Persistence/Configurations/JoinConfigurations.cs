using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CMC.Infrastructure.Persistence.Configurations;


public class LibraryControlFrameworkConfiguration : IEntityTypeConfiguration<LibraryControlFramework>
{
public void Configure(EntityTypeBuilder<LibraryControlFramework> b)
{
b.HasKey(x => new { x.ControlId, x.FrameworkId });
b.HasOne(x => x.Control).WithMany(c => c.FrameworkLinks).HasForeignKey(x => x.ControlId).OnDelete(DeleteBehavior.Cascade);
b.HasOne(x => x.Framework).WithMany(f => f.ControlLinks).HasForeignKey(x => x.FrameworkId).OnDelete(DeleteBehavior.Cascade);
}
}


public class LibraryControlScenarioConfiguration2 : IEntityTypeConfiguration<LibraryControlScenario>
{
public void Configure(EntityTypeBuilder<LibraryControlScenario> b)
{
b.HasKey(x => new { x.ControlId, x.ScenarioId });
b.Property(x => x.FrequencyEffect).HasPrecision(18, 4);
b.Property(x => x.ImpactEffect).HasPrecision(18, 4);
b.HasOne(x => x.Control).WithMany(c => c.ScenarioLinks).HasForeignKey(x => x.ControlId).OnDelete(DeleteBehavior.Cascade);
b.HasOne(x => x.Scenario).WithMany(s => s.ControlLinks).HasForeignKey(x => x.ScenarioId).OnDelete(DeleteBehavior.Cascade);
}
}


public class LibraryControlIndustryConfiguration2 : IEntityTypeConfiguration<LibraryControlIndustry>
{
public void Configure(EntityTypeBuilder<LibraryControlIndustry> b)
{
b.HasKey(x => new { x.ControlId, x.IndustryId });
b.HasOne(x => x.Control).WithMany(c => c.IndustryLinks).HasForeignKey(x => x.ControlId).OnDelete(DeleteBehavior.Cascade);
b.HasOne<Industry>().WithMany().HasForeignKey(x => x.IndustryId).OnDelete(DeleteBehavior.Cascade);
}
}


public class FrameworkIndustryConfiguration2 : IEntityTypeConfiguration<FrameworkIndustry>
{
public void Configure(EntityTypeBuilder<FrameworkIndustry> b)
{
b.HasKey(x => new { x.FrameworkId, x.IndustryId });
b.HasOne(x => x.Framework).WithMany(f => f.IndustryLinks).HasForeignKey(x => x.FrameworkId).OnDelete(DeleteBehavior.Cascade);
b.HasOne<Industry>().WithMany().HasForeignKey(x => x.IndustryId).OnDelete(DeleteBehavior.Cascade);
}
}


public class CustomerIndustryConfiguration2 : IEntityTypeConfiguration<CustomerIndustry>
{
public void Configure(EntityTypeBuilder<CustomerIndustry> b)
{
b.HasKey(x => new { x.CustomerId, x.IndustryId });
b.HasOne(x => x.Customer).WithMany(c => c.IndustryLinks).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
b.HasOne<Industry>().WithMany().HasForeignKey(x => x.IndustryId).OnDelete(DeleteBehavior.Cascade);
}
}


public class LibraryScenarioIndustryConfiguration2 : IEntityTypeConfiguration<LibraryScenarioIndustry>
{
public void Configure(EntityTypeBuilder<LibraryScenarioIndustry> b)
{
b.HasKey(x => new { x.ScenarioId, x.IndustryId });
b.HasOne(x => x.Scenario).WithMany(s => s.IndustryLinks).HasForeignKey(x => x.ScenarioId).OnDelete(DeleteBehavior.Cascade);
b.HasOne<Industry>().WithMany().HasForeignKey(x => x.IndustryId).OnDelete(DeleteBehavior.Cascade);
}
}
