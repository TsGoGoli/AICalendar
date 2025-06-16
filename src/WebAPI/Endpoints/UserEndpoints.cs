using AICalendar.Application.DTOs;
using AICalendar.Application.Features.Users.Commands.CreateUser;
using AICalendar.Application.Features.Users.Commands.DeleteUser;
using AICalendar.Application.Features.Users.Commands.UpdateUser;
using AICalendar.Application.Features.Users.Queries.GetUserById;
using AICalendar.Application.Features.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AICalendar.WebAPI.Endpoints;

/// <summary>
/// Extension methods for configuring User API endpoints
/// </summary>
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();

        // Get all users with optional filtering
        group.MapGet("/", async (IMediator mediator, string? name = null, string? email = null) =>
        {
            var query = new GetUsersQuery { NameFilter = name, EmailFilter = email };
            var result = await mediator.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetUsers")
        .WithDescription("Get all users with optional name and email filters")
        .Produces<IReadOnlyList<UserDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // Get user by ID
        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetUserByIdQuery(id);
            var result = await mediator.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Error);
        })
        .WithName("GetUserById")
        .WithDescription("Get a specific user by ID")
        .Produces<UserDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // Create new user
        group.MapPost("/", async (CreateUserDto userDto, IMediator mediator) =>
        {
            var command = new CreateUserCommand(userDto);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/users/{result.Value.Id}", result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("CreateUser")
        .WithDescription("Create a new user")
        .Produces<UserDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // Update existing user
        group.MapPut("/{id:guid}", async (Guid id, UpdateUserDto userDto, IMediator mediator) =>
        {
            var command = new UpdateUserCommand(id, userDto);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        .WithName("UpdateUser")
        .WithDescription("Update an existing user")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // Delete user
        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var command = new DeleteUserCommand(id);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        .WithName("DeleteUser")
        .WithDescription("Delete a user")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);

        return app;
    }
}