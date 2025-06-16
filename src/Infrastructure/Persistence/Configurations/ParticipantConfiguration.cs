using AICalendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AICalendar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Participant entity
/// </summary>
public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.ToTable("Participants");
        
        builder.HasKey(p => p.Id);
        
        // Composite unique index on EventId and UserId
        builder.HasIndex(p => new { p.EventId, p.UserId }).IsUnique();
        
        builder.Property(p => p.Status)
            .IsRequired();
            
        builder.Property(p => p.Note)
            .HasMaxLength(500);
            
        builder.Property(p => p.CreatedAt)
            .IsRequired();
            
        // Configure relationships
        builder.HasOne(p => p.Event)
            .WithMany(e => e.Participants)
            .HasForeignKey(p => p.EventId);
            
        builder.HasOne(p => p.User)
            .WithMany(u => u.Participations)
            .HasForeignKey(p => p.UserId);
    }
}