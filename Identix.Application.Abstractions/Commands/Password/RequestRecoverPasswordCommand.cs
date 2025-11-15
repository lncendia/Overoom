using MediatR;

namespace Identix.Application.Abstractions.Commands.Password;

/// <summary>
/// Команда для запроса восстановления пароля.
/// </summary>
public class RequestRecoverPasswordCommand : IRequest
{
    /// <summary>
    /// Электронная почта пользователя.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// URL для сброса пароля.
    /// </summary>
    public required string ResetUrl { get; init; }
    
    /// <summary>
    /// URL адрес возврата.
    /// </summary>
    public string? ReturnUrl { get; init; }
}