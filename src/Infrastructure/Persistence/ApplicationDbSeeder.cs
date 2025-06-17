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

            // Create an admin user
            var adminUser = new User("Admin User", "admin@aicalendar.com", "admin");
            adminUser.AddRole("Admin");
            adminUser.SetPassword("Admin123!"); // In a real app, use a secure password

            var users = new List<User>
            {
                adminUser,
                CreateUser("John Smith", "john.smith@example.com", "jsmith", "Password123!"),
                CreateUser("Jane Doe", "jane.doe@example.com", "jdoe", "Password123!"),
                CreateUser("Bob Johnson", "bob.johnson@example.com", "bjohnson", "Password123!")
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Seed Events and Participants
            var organizer = users[1]; // John is the organizer
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

            // Reload events from database to avoid concurrency issues
            var freshEvent1 = await _context.Events.Include(e => e.Participants).FirstAsync(e => e.Id == event1.Id);
            var freshEvent2 = await _context.Events.Include(e => e.Participants).FirstAsync(e => e.Id == event2.Id);
            
            // Add participants to events
            freshEvent1.AddParticipant(users[2], ParticipantStatus.Accepted);
            freshEvent1.AddParticipant(users[3], ParticipantStatus.Pending);
            
            freshEvent2.AddParticipant(users[2], ParticipantStatus.Tentative);
            freshEvent2.AddParticipant(users[3], ParticipantStatus.Accepted);

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Seed data added successfully");
        }
    }

    private User CreateUser(string name, string email, string username, string password)
    {
        var user = new User(name, email, username);
        user.SetPassword(password);
        return user;
    }
}