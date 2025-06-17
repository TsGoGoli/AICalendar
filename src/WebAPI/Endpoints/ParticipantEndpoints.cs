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
public static class Participant