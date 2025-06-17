using AICalendar.Application.DTOs;
using AICalendar.Application.Features.Participants.Commands.AddParticipant;
using AICalendar.Application.Features.Participants.Commands.RemoveParticipant;
using AICalendar.Application.Features.Participants.Commands.UpdateParticipantStatus;
using AICalendar.Application.Features.Participants.Queries.GetEventParticipants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AICalendar.WebAPI.Endpoints;

/// <summary>
/// Extension methods for configuring Participant API endpoints
/// </summary>
public static class ParticipantEndpoints
{
    public static IEndpointRouteBuilder MapParticipantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events/{eventId:guid}/participants")
            .WithTags("Participants")
            .WithOpenApi()
            .RequireAuthorization(); // Require authentication for all endpoints in this group

        // Get all participants for an event
        group.MapGet("/", async (Guid eventId, IMediator mediator) =>
        {
            var query = new GetEventParticipantsQuery(eventId);
            var result = await mediator.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetEventParticipants")
        .WithDescription("Get all participants for a specific event")
        .Produces<IReadOnlyList<ParticipantDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        // Add participant to an event
        group.MapPost("/", async (Guid eventId, AddParticipantDto participantDto, IMediator mediator) =>
        {
            var command = new AddParticipantCommand(eventId, participantDto);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/events/{eventId}/participants/{result.Value.UserId}", result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("AddParticipant")
        .WithDescription("Add a participant to an event")
        .Produces<ParticipantDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        // Update participant status
        group.MapPut("/{userId:guid}", async (Guid eventId, Guid userId, UpdateParticipantStatusDto statusDto, IMediator mediator) =>
        {
            var command = new UpdateParticipantStatusCommand(eventId, userId, statusDto);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("UpdateParticipantStatus")
        .WithDescription("Update a participant's status")
        .Produces<ParticipantDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        // Remove participant from an event
        group.MapDelete("/{userId:guid}", async (Guid eventId, Guid userId, IMediator mediator) =>
        {
            var command = new RemoveParticipantCommand(eventId, userId);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        .WithName("RemoveParticipant")
        .WithDescription("Remove a participant from an event")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}