using AICalendar.Domain.Common;
using System.Security.Cryptography;
using System.Text;

namespace AICalendar.Domain.Entities;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User : BaseEntity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string? Username { get; private set; }
    public string? PasswordHash { get; private set; }
    public string? PasswordSalt { get; private set; }
    public List<string> Roles { get; private set; } = new List<string>();
    
    // Navigation properties for EF Core
    public virtual ICollection<Participant> Participations { get; private set; } = new List<Participant>();

    // Required by EF Core
    protected User() : base() { }
    
    public User(string name, string email, string? username = null) : base()
    {
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Name cannot be empty", nameof(name));
        Email = !string.IsNullOrWhiteSpace(email) ? email : throw new ArgumentException("Email cannot be empty", nameof(email));
        Username = username;
        Roles.Add("User"); // Default role
    }

    public User(Guid id, string name, string email, string? username = null) : base(id)
    {
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Name cannot be empty", nameof(name));
        Email = !string.IsNullOrWhiteSpace(email) ? email : throw new ArgumentException("Email cannot be empty", nameof(email));
        Username = username;
        Roles.Add("User"); // Default role
    }

    public void Update(string name, string email, string? username = null)
    {
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Name cannot be empty", nameof(name));
        Email = !string.IsNullOrWhiteSpace(email) ? email : throw new ArgumentException("Email cannot be empty", nameof(email));
        Username = username;
        
        SetLastModified();
    }

    public void SetPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));
            
        var salt = GenerateSalt();
        PasswordSalt = Convert.ToBase64String(salt);
        PasswordHash = HashPassword(password, salt);
        
        SetLastModified();
    }
    
    public bool VerifyPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(PasswordHash) || string.IsNullOrWhiteSpace(PasswordSalt))
            return false;
            
        var salt = Convert.FromBase64String(PasswordSalt);
        var hash = HashPassword(password, salt);
        
        return hash == PasswordHash;
    }
    
    public void AddRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be empty", nameof(role));
            
        if (!Roles.Contains(role))
        {
            Roles.Add(role);
            SetLastModified();
        }
    }
    
    public void RemoveRole(string role)
    {
        if (Roles.Contains(role))
        {
            Roles.Remove(role);
            SetLastModified();
        }
    }
    
    public bool HasRole(string role)
    {
        return Roles.Contains(role);
    }
    
    private static byte[] GenerateSalt()
    {
        var rng = RandomNumberGenerator.Create();
        var salt = new byte[16];
        rng.GetBytes(salt);
        return salt;
    }
    
    private static string HashPassword(string password, byte[] salt)
    {
        using var sha256 = SHA256.Create();
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var combinedBytes = new byte[passwordBytes.Length + salt.Length];
        
        Array.Copy(passwordBytes, 0, combinedBytes, 0, passwordBytes.Length);
        Array.Copy(salt, 0, combinedBytes, passwordBytes.Length, salt.Length);
        
        var hashBytes = sha256.ComputeHash(combinedBytes);
        return Convert.ToBase64String(hashBytes);
    }
}