using AICalendar.Domain.Common;
using AICalendar.Domain.Repositories;
using AICalendar.Domain.Services;
using AICalendar.Domain.ValueObjects;
using MediatR;

namespace AICalendar.Application.Features.Events.Queries.FindAvailableSlots;

/// <summary>
/// Handler for FindAvailableSlotsQuery
/// </summary>
public class FindAvailableSlotsQueryHandler : IRequestHandler<FindAvailableSlotsQuery, Result<IReadOnlyList<DateTimeRange>>>
{
    private readonly IUserRepository _userRepository;
    private readonly ISchedulingService _schedulingService;

    public FindAvailableSlotsQueryHandler(
        IUserRepository userRepository,
        ISchedulingService schedulingService)
    {
        _userRepository = userRepository;
        _schedulingService = schedulingService;
    }

    public async Task<Result<IReadOnlyList<DateTimeRange>>> Handle(FindAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all users from the request
            var users = new List<Domain.Entities.User>();
            
            foreach (var userId in request.UserIds)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result.Failure<IReadOnlyList<DateTimeRange>>($"User with ID {userId} not found");
                }
                
                users.Add(user);
            }
            
            if (!users.Any())
            {
                return Result.Failure<IReadOnlyList<DateTimeRange>>("At least one user must be provided");
            }
            
            // Create search range and find available slots
            var searchRange = request.GetSearchRange();
            var result = _schedulingService.FindAvailableSlots(
                users, 
                searchRange, 
                request.Duration, 
                request.MaxResults);
                
            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<DateTimeRange>>($"Failed to find available slots: {ex.Message}");
        }
    }
}