using AICalendar.Domain.Common;
using AICalendar.Domain.Repositories;
using MediatR;

namespace AICalendar.Application.Features.Events.Commands.DeleteEvent;

/// <summary>
/// Handler for DeleteEventCommand
/// </summary>
public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Result>
{
    private readonly IEventRepository _eventRepository;

    public DeleteEventCommandHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Result> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if event exists
            var @event = await _eventRepository.GetByIdAsync(request.Id);
            if (@event == null)
            {
                return Result.Failure($"Event with ID {request.Id} not found");
            }

            // Delete event
            return await _eventRepository.DeleteAsync(request.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete event: {ex.Message}");
        }
    }
}