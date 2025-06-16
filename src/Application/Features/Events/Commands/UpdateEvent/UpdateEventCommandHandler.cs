using AICalendar.Domain.Common;
using AICalendar.Domain.Repositories;
using AICalendar.Domain.ValueObjects;
using MediatR;

namespace AICalendar.Application.Features.Events.Commands.UpdateEvent;

/// <summary>
/// Handler for UpdateEventCommand
/// </summary>
public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Result>
{
    private readonly IEventRepository _eventRepository;

    public UpdateEventCommandHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Result> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the event to update
            var @event = await _eventRepository.GetByIdAsync(request.Id);
            if (@event == null)
            {
                return Result.Failure($"Event with ID {request.Id} not found");
            }

            // Update event properties
            var timeRange = new DateTimeRange(request.Start, request.End);
            @event.Update(request.Title, timeRange, request.Description);

            // Update event status if specified
            if (request.Status.HasValue)
            {
                switch (request.Status.Value)
                {
                    case Domain.Enums.EventStatus.Cancelled:
                        @event.Cancel();
                        break;
                    case Domain.Enums.EventStatus.Completed:
                        @event.Complete();
                        break;
                }
            }

            // Save changes
            return await _eventRepository.UpdateAsync(@event);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update event: {ex.Message}");
        }
    }
}