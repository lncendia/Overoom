using Identix.Application.Abstractions.Entities;
using MediatR;

namespace Identix.Application.Abstractions.Commands.Authentication;

/// <summary>
/// Команда для аутентификации пользователя по паролю.
/// </summary>
public class AuthenticateUserByPasswordCommand : IRequest<AppUser>
{
    /// <summary>
    /// Электронная почту пользователя.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Пароль пользователя.
    /// </summary>
    public required string Password { get; init; }
    
    /// <summary>
    /// URL для подтверждения пользователя.
    /// </summary>
    public required string ConfirmUrl { get; init; }
    
    /// <summary>
    /// URL адрес возврата
    /// </summary>
    public string? ReturnUrl { get; init; }
}