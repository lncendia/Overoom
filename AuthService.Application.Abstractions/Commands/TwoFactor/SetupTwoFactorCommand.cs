using AuthService.Application.Abstractions.Entities;
using MediatR;

namespace AuthService.Application.Abstractions.Commands.TwoFactor;

/// <summary>
/// Команда для получения аутентификатора для подключения 2FA
/// </summary>
public class SetupTwoFactorCommand : IRequest<(UserData user, string token)>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; init; }
}