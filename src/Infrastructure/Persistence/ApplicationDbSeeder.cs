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
            // Only seed if the database is empty
            if (!await _context.Users.AnyAsync())
            {
                _logger.LogInformation("Starting database seeding...");
                
                // Add users
                _logger.LogInformation("Seeding users...");
                var users = AddUsers();
                
                // Add events
                _logger.LogInformation("Seeding events...");
                var events = AddEvents(users[1]); // John as organizer
                
                // Add participants
                _logger.LogInformation("Seeding participants...");
                AddParticipants(events, users);
                
                // Save all changes at once
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Database seeding completed successfully");
            }
            else
            {
                _logger.LogInformation("Database already contains data, skipping seed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
    
    private List<User> AddUsers()
    {
        // Create an admin user
        var adminUser = new User("Admin User", "admin@aicalendar.com", "admin");
        adminUser.AddRole("Admin");
        adminUser.SetPassword("Admin123!"); // In a real app, use a secure password
        
        // Create regular users
        var johnUser = CreateUser("John Smith", "john.smith@example.com", "jsmith", "Password123!");
        var janeUser = CreateUser("Jane Doe", "jane.doe@example.com", "jdoe", "Password123!");
        var bobUser = CreateUser("Bob Johnson", "bob.johnson@example.com", "bjohnson", "Password123!");
        
        var users = new List<User> { adminUser, johnUser, janeUser, bobUser };
        _context.Users.AddRange(users);
        
        _logger.LogInformation("Added {Count} users to database", users.Count);
        return users;
    }
    
    private List<Event> AddEvents(User organizer)
    {
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
        
        var events = new List<Event> { event1, event2 };
        _context.Events.AddRange(events);
        
        _logger.LogInformation("Added {Count} events to database", events.Count);
        return events;
    }
    
    private void AddParticipants(List<Event> events, List<User> users)
    {
        // Create participants using ID constructor
        var participants = new List<Participant>
        {
            // For event 1
            new Participant(events[0].Id, users[2].Id, ParticipantStatus.Accepted),
            new Participant(events[0].Id, users[3].Id, ParticipantStatus.Pending),
            
            // For event 2
            new Participant(events[1].Id, users[2].Id, ParticipantStatus.Tentative),
            new Participant(events[1].Id, users[3].Id, ParticipantStatus.Accepted)
        };
        
        _context.Participants.AddRange(participants);
        
        _logger.LogInformation("Added {Count} participants to database", participants.Count);
    }

    private User CreateUser(string name, string email, string username, string password)
    {
        var user = new User(name, email, username);
        user.SetPassword(password);
        return user;
    }
}