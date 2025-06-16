using AICalendar.Domain.Entities;
using AICalendar.Domain.Enums;
using AICalendar.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AICalendar.Infrastructure.Persistence;

/// <summary>
/// Class to seed initial data to the database
/// </summary>
public class ApplicationDbSeeder
{
    private readonly ILogger<ApplicationDbSeeder> _logger;
    private readonly ApplicationDbContext _context;

    public ApplicationDbSeeder(ILogger<ApplicationDbSeeder> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // Seed Users if none exist
        if (!await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Seeding users...");

            var users = new List<User>
            {
                new User("John Smith", "john.smith@example.com", "jsmith"),
                new User("Jane Doe", "jane.doe@example.com", "jdoe"),
                new User("Bob Johnson", "bob.johnson@example.com", "bjohnson")
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Seed Events and Participants
            var organizer = users[0]; // John is the organizer
            var now = DateTime.UtcNow;
            var tomorrow = now.AddDays(1);
            var nextWeek = now.AddDays(7);

            var event1 = new Event(
                "Team Meeting", 
                new DateTimeRange(tomorrow.Date.AddHours(10), tomorrow.Date.AddHours(11)),
                organizer,
                "Weekly team sync-up");

            var event2 = new Event(
                "Project Review", 
                new DateTimeRange(nextWeek.Date.AddHours(14), nextWeek.Date.AddHours(16)),
                organizer,
                "End of sprint project review");

            await _context.Events.AddRangeAsync(event1, event2);
            await _context.SaveChangesAsync();

            // Add participants to events
            event1.AddParticipant(users[1], ParticipantStatus.Accepted);
            event1.AddParticipant(users[2], ParticipantStatus.Pending);
            
            event2.AddParticipant(users[1], ParticipantStatus.Tentative);
            event2.AddParticipant(users[2], ParticipantStatus.Accepted);

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Seed data added successfully");
        }
    }
}