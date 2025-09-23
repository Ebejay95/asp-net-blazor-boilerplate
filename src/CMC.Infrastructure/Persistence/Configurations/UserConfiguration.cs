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

        // Password Reset Properties
        b.Property(u => u.PasswordResetToken).HasMaxLength(100);
        b.Property(u => u.PasswordResetTokenExpiry);

        // 2FA Properties
        b.Property(u => u.TwoFASecret)
            .HasMaxLength(64)
            .HasComment("Base32-encoded TOTP secret for two-factor authentication");

        b.Property(u => u.TwoFAEnabledAt)
            .HasComment("Timestamp when 2FA was first enabled");

        b.Property(u => u.TwoFABackupCodes)
            .HasMaxLength(1000)
            .HasComment("Comma-separated backup codes for 2FA recovery");

        // Computed property for TwoFAEnabled (read-only)
        b.Ignore(u => u.TwoFAEnabled);

        // Relationships
        b.HasOne(u => u.Customer)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(u => u.CustomerId).HasDatabaseName("IX_Users_CustomerId");

        // Additional indexes for performance
        b.HasIndex(u => u.TwoFASecret).HasDatabaseName("IX_Users_TwoFASecret");
        b.HasIndex(u => u.PasswordResetToken).HasDatabaseName("IX_Users_PasswordResetToken");
        b.HasIndex(u => u.CreatedAt).HasDatabaseName("IX_Users_CreatedAt");
        b.HasIndex(u => u.LastLoginAt).HasDatabaseName("IX_Users_LastLoginAt");
    }
}
