namespace AICalendar.Domain.ValueObjects;

/// <summary>
/// Represents a range of time with start and end datetime
/// </summary>
public sealed class DateTimeRange
{
    public DateTime Start { get; }
    public DateTime End { get; }
    public TimeSpan Duration => End - Start;

    public DateTimeRange(DateTime start, DateTime end)
    {
        if (end < start)
        {
            throw new ArgumentException("End date cannot be earlier than start date", nameof(end));
        }

        Start = start.ToUniversalTime();
        End = end.ToUniversalTime();
    }

    public DateTimeRange(DateTime start, TimeSpan duration) 
        : this(start, start.Add(duration))
    {
    }

    public bool Overlaps(DateTimeRange other)
    {
        return Start < other.End && other.Start < End;
    }

    public bool Contains(DateTime dateTime)
    {
        return Start <= dateTime && dateTime <= End;
    }

    public bool Contains(DateTimeRange other)
    {
        return Start <= other.Start && other.End <= End;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || obj is not DateTimeRange other)
            return false;

        return Start.Equals(other.Start) && End.Equals(other.End);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    public override string ToString()
    {
        return $"{Start:yyyy-MM-dd HH:mm} - {End:yyyy-MM-dd HH:mm}";
    }
}