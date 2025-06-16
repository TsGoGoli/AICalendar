namespace AICalendar.Domain.Common;

/// <summary>
/// Represents the result of an operation with a status and optional error message
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string? error = null)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("A successful result cannot have an error message");
            
        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("A failure result must have an error message");
            
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true);
    
    public static Result Failure(string error) => new(false, error);
    
    public static Result<T> Success<T>(T value) => new(value);
    
    public static Result<T> Failure<T>(string error) => new(error);
}

/// <summary>
/// Represents the result of an operation with a value and status
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;
    
    public T Value 
    { 
        get 
        {
            if (IsFailure)
                throw new InvalidOperationException("Cannot access value of a failed result");
                
            return _value!;
        }
    }

    protected internal Result(T value) : base(true)
    {
        _value = value;
    }

    protected internal Result(string error) : base(false, error)
    {
        _value = default;
    }
}