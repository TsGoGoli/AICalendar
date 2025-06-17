using AICalendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AICalendar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for User entity
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(u => u.Email)
            .IsUnique();
            
        builder.Property(u => u.Username)
            .HasMaxLength(50);
            
        builder.Property(u => u.PasswordHash)
            .HasMaxLength(256);
            
        builder.Property(u => u.PasswordSalt)
            .HasMaxLength(256);
            
        builder.Property(u => u.CreatedAt)
            .IsRequired();
            
        // Configure Roles collection
        builder.Property(u => u.Roles)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
    }
}