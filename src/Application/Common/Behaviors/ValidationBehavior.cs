using AICalendar.Domain.Common;
using FluentValidation;
using MediatR;

namespace AICalendar.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for validation of requests
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        
        // Run all validators
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        
        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToList();

        if (failures.Any())
        {
            var errorMessages = string.Join("\n", failures.Select(f => f.ErrorMessage));
            
            // Create a failure result with validation errors
            object? failureResult;

            if (typeof(TResponse).IsGenericType && 
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                // If the response is of type Result<T>
                var resultType = typeof(TResponse).GetGenericArguments()[0];
                var failureMethod = typeof(Result)
                    .GetMethod(nameof(Result.Failure), new[] { typeof(string) })
                    ?.MakeGenericMethod(resultType);
                
                failureResult = failureMethod?.Invoke(null, new object[] { errorMessages });
            }
            else
            {
                // If the response is of type Result
                failureResult = Result.Failure(errorMessages);
            }
            
            return (TResponse)failureResult!;
        }

        // Continue to the next behavior or handler
        return await next();
    }
}