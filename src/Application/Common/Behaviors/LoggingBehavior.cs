using MediatR;
using Microsoft.Extensions.Logging;

namespace AICalendar.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for logging requests and responses
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        _logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var result = await next();
            _logger.LogInformation("Handled {RequestName}", requestName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestName}: {ErrorMessage}", requestName, ex.Message);
            throw;
        }
    }
}