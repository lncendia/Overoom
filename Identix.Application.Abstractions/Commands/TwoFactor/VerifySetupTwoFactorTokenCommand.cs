using MediatR;

namespace Identix.Application.Abstractions.Commands.TwoFactor;

/// <summary>
/// Команда верификации подключения пользователю 2FA
/// </summary>
public class VerifySetupTwoFactorTokenCommand : IRequest<IReadOnlyCollection<string>>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Код верификации 2FA
    /// </summary>
    public required string Code { get; init; }
}