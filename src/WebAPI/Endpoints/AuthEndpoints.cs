using AICalendar.Application.Common.Services;
using AICalendar.Application.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AICalendar.WebAPI.Endpoints;

/// <summary>
/// Extension methods for configuring Authentication API endpoints
/// </summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // Login endpoint
        group.MapPost("/login", async (LoginDto loginDto, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(loginDto);

            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }
            
            return Results.BadRequest(new { message = result.Error });
        })
        .WithName("Login")
        .WithDescription("Authenticate a user and get a JWT token")
        .Produces<AuthResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // Register endpoint
        group.MapPost("/register", async (RegisterDto registerDto, IAuthService authService) =>
        {
            var result = await authService.RegisterAsync(registerDto);

            if (result.IsSuccess)
            {
                return Results.Created("/api/auth/login", result.Value);
            }
            
            return Results.BadRequest(new { message = result.Error });
        })
        .WithName("Register")
        .WithDescription("Register a new user")
        .Produces<AuthResponseDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        return app;
    }
}