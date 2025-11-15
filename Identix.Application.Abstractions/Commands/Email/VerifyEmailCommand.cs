using MediatR;

namespace Identix.Application.Abstractions.Commands.Email;

/// <summary>
/// Команда для проверки электронной почты пользователя.
/// </summary>
public class VerifyEmailCommand : IRequest
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Код проверки.
    /// </summary>
    public required string Code { get; init; }
}