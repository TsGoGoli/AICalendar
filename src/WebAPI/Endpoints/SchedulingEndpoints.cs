using AICalendar.Application.Features.Events.Queries.FindAvailableSlots;
using AICalendar.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AICalendar.WebAPI.Endpoints;

/// <summary>
/// Extension methods for configuring Scheduling API endpoints
/// </summary>
public static class SchedulingEndpoints
{
    public static IEndpointRouteBuilder MapSchedulingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/scheduling")
            .WithTags("Scheduling")
            .WithOpenApi();

        // Find available time slots for a group of users
        group.MapPost("/find-available-slots", async (FindAvailableSlotsRequest request, IMediator mediator) =>
        {
            var query = new FindAvailableSlotsQuery
            {
                UserIds = request.UserIds,
                Start = request.Start,
                End = request.End,
                Duration = TimeSpan.FromMinutes(request.DurationMinutes),
                MaxResults = request.MaxResults ?? 5
            };
            
            var result = await mediator.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value.Select(r => new TimeSlotResponse
                {
                    Start = r.Start,
                    End = r.End
                }))
                : Results.BadRequest(result.Error);
        })
        .WithName("FindAvailableSlots")
        .WithDescription("Find available time slots for a group of users")
        .Produces<IEnumerable<TimeSlotResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    // Request and response models for scheduling endpoints
    public class FindAvailableSlotsRequest
    {
        public List<Guid> UserIds { get; set; } = new();
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int DurationMinutes { get; set; } = 60;
        public int? MaxResults { get; set; }
    }

    public class TimeSlotResponse
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}