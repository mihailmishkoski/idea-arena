using MediatR;

namespace BusinessIdea.Application.Features.Comments.Commands.DeleteComment;

/// <summary>Deletes a comment. Only the comment author may do this.</summary>
public record DeleteCommentCommand(Guid Id) : IRequest;
