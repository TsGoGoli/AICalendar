using AICalendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AICalendar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Event entity
/// </summary>
public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(e => e.Description)
            .HasMaxLength(2000);
            
        // Configure TimeRange value object as owned entity
        builder.OwnsOne(e => e.TimeRange, timeRange =>
        {
            timeRange.Property(tr => tr.Start)
                .HasColumnName("StartTime")
                .IsRequired();
                
            timeRange.Property(tr => tr.End)
                .HasColumnName("EndTime")
                .IsRequired();
        });
        
        builder.Property(e => e.Status)
            .IsRequired();
            
        builder.Property(e => e.CreatedAt)
            .IsRequired();
            
        // Configure relationship with User (Event - Organizer)
        builder.HasOne(e => e.Organizer)
            .WithMany()
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}