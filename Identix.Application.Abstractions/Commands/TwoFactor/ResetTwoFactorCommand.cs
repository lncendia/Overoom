using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Enums;
using MediatR;

namespace Identix.Application.Abstractions.Commands.TwoFactor;

/// <summary>
/// Команда для сброса 2FA
/// </summary>
public class ResetTwoFactorCommand : IRequest<AppUser>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Код верификации 2FA
    /// </summary>
    public required string Code { get; init; }
    
    /// <summary>
    /// Тип кода для прохождения 2FA
    /// </summary>
    public required CodeType Type { get; init; }
}