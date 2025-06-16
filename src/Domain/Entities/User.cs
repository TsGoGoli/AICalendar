using AICalendar.Domain.Common;

namespace AICalendar.Domain.Entities;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User : BaseEntity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string? Username { get; private set; }
    
    // Navigation properties for EF Core
    public virtual ICollection<Participant> Participations { get; private set; } = new List<Participant>();

    // Required by EF Core
    protected User() : base() { }
    
    public User(string name, string email, string? username = null) : base()
    {
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Name cannot be empty", nameof(name));
        Email = !string.IsNullOrWhiteSpace(email) ? email : throw new ArgumentException("Email cannot be empty", nameof(email));
        Username = username;
    }

    public User(Guid id, string name, string email, string? username = null) : base(id)
    {
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Name cannot be empty", nameof(name));
        Email = !string.IsNullOrWhiteSpace(email) ? email : throw new ArgumentException("Email cannot be empty", nameof(email));
        Username = username;
    }

    public void Update(string name, string email, string? username = null)
    {
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Name cannot be empty", nameof(name));
        Email = !string.IsNullOrWhiteSpace(email) ? email : throw new ArgumentException("Email cannot be empty", nameof(email));
        Username = username;
        
        SetLastModified();
    }
}