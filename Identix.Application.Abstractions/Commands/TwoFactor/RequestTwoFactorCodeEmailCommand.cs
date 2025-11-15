using MediatR;

namespace Identix.Application.Abstractions.Commands.TwoFactor;
/// <summary>
/// Команда для отправки кода 2FA на почту.
/// </summary>
public class RequestTwoFactorCodeEmailCommand : IRequest
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; init; }
}