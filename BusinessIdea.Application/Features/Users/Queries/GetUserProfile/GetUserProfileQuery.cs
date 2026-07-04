using MediatR;

namespace BusinessIdea.Application.Features.Users.Queries.GetUserProfile;

public record GetUserProfileQuery(string UserId) : IRequest<UserProfileDto>;
