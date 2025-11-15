using Identix.Application.Abstractions.Entities;
using MediatR;

namespace Identix.Application.Abstractions.Commands.Email;

/// <summary>
/// Команда для изменения электронной почты пользователя.
/// </summary>
public class ChangeEmailCommand : IRequest<AppUser>
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Код подтверждения.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Новая электронная почта пользователя.
    /// </summary>
    public required string NewEmail { get; init; }
}