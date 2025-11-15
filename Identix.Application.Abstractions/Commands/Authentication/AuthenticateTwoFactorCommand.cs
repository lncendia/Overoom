using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Enums;
using MediatR;

namespace Identix.Application.Abstractions.Commands.Authentication;

/// <summary>
/// Команда для прохождения пользователем 2FA
/// </summary>
public class AuthenticateTwoFactorCommand : IRequest<AppUser>
{
    /// <summary>
    /// Код для прохождения 2FA
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Тип кода для прохождения 2FA
    /// </summary>
    public required CodeType Type { get; init; }
}