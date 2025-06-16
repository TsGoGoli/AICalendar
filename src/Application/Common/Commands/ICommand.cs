using AICalendar.Domain.Common;
using MediatR;

namespace AICalendar.Application.Common.Commands;

/// <summary>
/// Base interface for command without a return value
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Base interface for command with a return value
/// </summary>
/// <typeparam name="TResponse">The type of the response</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}