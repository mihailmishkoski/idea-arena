using MediatR;

namespace BusinessIdea.Application.Features.Chat.Commands.ApplyToCofound;

/// <summary>
/// A co-founder application for an idea. Unlike a bare chat request, it
/// carries an (all-optional) structured pitch about the applicant which is
/// delivered as the conversation's first message, an in-app notification and
/// an email — so the idea's author knows exactly who wants to join and why.
/// Returns the conversation id.
/// </summary>
public record ApplyToCofoundCommand(
    Guid PostId,
    string? Role,
    string? Skills,
    string? Motivation,
    string? Availability,
    string? ContactLink) : IRequest<Guid>;
