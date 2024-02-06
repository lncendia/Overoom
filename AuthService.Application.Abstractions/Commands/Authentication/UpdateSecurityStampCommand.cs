using AuthService.Application.Abstractions.Entities;
using MediatR;

namespace AuthService.Application.Abstractions.Commands.Authentication;

/// <summary>
/// Команда на закрытие остальных сессий пользователя
/// </summary>
public class UpdateSecurityStampCommand : IRequest<UserData>
{
    /// <summary>
    /// Получает или задает идентификатор пользователя.
    /// </summary>
    public required Guid UserId { get; init; }
}