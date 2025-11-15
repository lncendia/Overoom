using MediatR;

namespace Identix.Application.Abstractions.Commands.Email;

/// <summary>
/// Команда для запроса изменения электронной почты.
/// </summary>
public class RequestChangeEmailCommand : IRequest
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public required Guid UserId { get; init; }
    
    /// <summary>
    /// Пароль
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Новая электронную почту пользователя.
    /// </summary>
    public required string NewEmail { get; init; }

    /// <summary>
    /// URL для сброса изменения электронной почты.
    /// </summary>
    public required string ResetUrl { get; init; }
    
    /// <summary>
    /// URL для возврата.
    /// </summary>
    public string ReturnUrl { get; init; } = "/";
}