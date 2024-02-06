using AuthService.Application.Abstractions.Entities;
using MediatR;

namespace AuthService.Application.Abstractions.Commands.Profile;

public class ChangeAvatarCommand : IRequest<UserData>
{
    public required Guid UserId { get; init; }
    public required Stream Avatar { get; init; }
}