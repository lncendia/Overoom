using Identix.Application.Abstractions.Entities;
using MediatR;

namespace Identix.Application.Abstractions.Commands.Authentication;

/// <summary>
/// Команда для аутентификации пользователя по паролю.
/// </summary>
public class AuthenticateUserByPasswordCommand : IRequest<AppUser>
{
    /// <summary>
    /// Получает или задает электронную почту пользователя.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Получает или задает пароль пользователя.
    /// </summary>
    public required string Password { get; init; }
}