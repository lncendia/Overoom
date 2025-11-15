using Identix.Application.Abstractions.Entities;
using MediatR;

namespace Identix.Application.Abstractions.Commands.Authentication;

/// <summary>
/// Команда для аутентификации пользователя через внешний провайдер.
/// </summary>
public class AuthenticateUserByExternalProviderCommand : IRequest<AppUser>
{
    /// <summary>
    /// Провайдер аутентификации.
    /// </summary>
    public required string LoginProvider { get; init; }

    /// <summary>
    /// Ключ провайдера аутентификации.
    /// </summary>
    public required string ProviderKey { get; init; }
}