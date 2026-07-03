using IdentityService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(u => u.Name).HasMaxLength(200).IsRequired();

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(u => u.FailedLoginAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.LockedUntil)
            .IsRequired(false);

        builder.Metadata.FindNavigation(nameof(User.RefreshTokens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}