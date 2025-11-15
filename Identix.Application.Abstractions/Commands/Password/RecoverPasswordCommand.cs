using MediatR;

namespace Identix.Application.Abstractions.Commands.Password;

/// <summary>
/// Команда для восстановления пароля.
/// </summary>
public class RecoverPasswordCommand : IRequest
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Код для восстановления пароля.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Новый пароль пользователя.
    /// </summary>
    public required string NewPassword { get; init; }
}