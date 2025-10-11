using MediatR;

namespace Identix.Application.Abstractions.Commands.OpenId;

/// <summary>
/// Команда для отзыва (аннулирования) гранта авторизации пользователя.
/// </summary>
public class RevokeGrantCommand : IRequest
{
    /// <summary>
    /// Идентификатор гранта (авторизации), который нужно отозвать.
    /// </summary>
    public required string GrantId { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя, которому должен принадлежать грант.
    /// </summary>
    public required Guid UserId { get; init; }
}
