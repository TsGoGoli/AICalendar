using AICalendar.Application.DTOs;
using AICalendar.Application.Features.Events.Commands.CreateEvent;
using AICalendar.Application.Features.Events.Commands.DeleteEvent;
using AICalendar.Application.Features.Events.Commands.UpdateEvent;
using AICalendar.Application.Features.Events.Queries.GetEventById;
using AICalendar.Application.Features.Events.Queries.GetEvents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AICalendar.WebAPI.Endpoints;

/// <summary>
/// Extension methods for configuring Event API endpoints
/// </summary>
public static class EventEndpoints
{
    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events")
            .WithTags("Events")
            .WithOpenApi()
            .RequireAuthorization(); // Require authentication for all endpoints in this group

        // Get all events with optional filtering
        group.MapGet("/", async (IMediator mediator, DateTime? startDate = null, DateTime? endDate = null, int? status = null) =>
        {
            var query = new GetEventsQuery 
            { 
                StartDate = startDate, 
                EndDate = endDate,
                Status = status.HasValue ? (Domain.Enums.EventStatus)status.Value : null
            };
            
            var result = await mediator.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetEvents")
        .WithDescription("Get all events with optional date range and status filters")
        .Produces<IReadOnlyList<EventDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        // Get event by ID
        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetEventByIdQuery(id);
            var result = await mediator.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Error);
        })
        .WithName("GetEventById")
        .WithDescription("Get a specific event by ID")
        .Produces<EventDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);

        // Create new event
        group.MapPost("/", async (CreateEventDto eventDto, IMediator mediator, Guid organizerId) =>
        {
            var command = new CreateEventCommand(eventDto, organizerId);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/events/{result.Value.Id}", result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("CreateEvent")
        .WithDescription("Create a new event")
        .Produces<EventDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        // Update existing event
        group.MapPut("/{id:guid}", async (Guid id, UpdateEventDto eventDto, IMediator mediator) =>
        {
            var command = new UpdateEventCommand(id, eventDto);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        .WithName("UpdateEvent")
        .WithDescription("Update an existing event")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        // Delete event
        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var command = new DeleteEventCommand(id);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        .WithName("DeleteEvent")
        .WithDescription("Delete an event")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}