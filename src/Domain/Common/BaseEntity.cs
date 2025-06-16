namespace AICalendar.Domain.Common;

/// <summary>
/// Base class for all entities
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    
    public DateTime CreatedAt { get; protected set; }
    
    public DateTime? LastModifiedAt { get; protected set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    protected BaseEntity(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetLastModified()
    {
        LastModifiedAt = DateTime.UtcNow;
    }
}