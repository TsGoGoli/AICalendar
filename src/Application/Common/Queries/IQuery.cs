using AICalendar.Domain.Common;
using MediatR;

namespace AICalendar.Application.Common.Queries;

/// <summary>
/// Base interface for all queries
/// </summary>
/// <typeparam name="TResponse">The type of the response</typeparam>
public interface IQuery<out TResponse> : IRequest<Result<TResponse>>
{
}