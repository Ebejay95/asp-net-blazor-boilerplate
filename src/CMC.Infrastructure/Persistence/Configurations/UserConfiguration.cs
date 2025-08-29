using CMC.Domain.Entities;
using CMC.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;


namespace CMC.Infrastructure.Persistence.Configurations;


public class UserConfiguration : IEntityTypeConfiguration<User>
{
public void Configure(EntityTypeBuilder<User> b)
{
b.HasKey(u => u.Id);


var emailComparer = new ValueComparer<Email>(
(l, r) => ((l == null ? "" : l.Value) == (r == null ? "" : r.Value)),
v => (v == null ? "" : v.Value).GetHashCode(),
v => v == null ? new Email("placeholder@example.invalid") : new Email(v.Value)
);


var emailProp = b.Property(u => u.Email)
.HasConversion(to => to.Value, from => new Email(from));
emailProp.Metadata.SetValueComparer(emailComparer);
emailProp.IsRequired().IsUnicode(false).HasMaxLength(320);


b.HasIndex(u => u.Email).IsUnique();


b.Property(u => u.PasswordHash).IsRequired();
b.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
b.Property(u => u.LastName).IsRequired().HasMaxLength(100);
b.Property(u => u.Role).HasMaxLength(100);
b.Property(u => u.Department).HasMaxLength(100);


b.HasOne(u => u.Customer)
.WithMany(c => c.Users)
.HasForeignKey(u => u.CustomerId)
.OnDelete(DeleteBehavior.SetNull);


b.HasIndex(u => u.CustomerId).HasDatabaseName("IX_Users_CustomerId");
}
}
