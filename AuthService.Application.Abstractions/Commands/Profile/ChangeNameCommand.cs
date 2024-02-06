using AuthService.Application.Abstractions.Entities;
using MediatR;

namespace AuthService.Application.Abstractions.Commands.Profile;

public class ChangeNameCommand : IRequest<UserData>
{
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
}