using System.Collections.Immutable;
using System.Security.Claims;
using MediatR;

namespace Identix.Application.Abstractions.Commands.OpenId;

/// <summary>
/// Команда для авторизации пользователя и создания ClaimsPrincipal с утверждениями (claims)
/// </summary>
public class AuthorizeUserCommand : IRequest<ClaimsPrincipal>
{
    /// <summary>
    /// Уникальный идентификатор пользователя, подлежащего авторизации
    /// </summary>
    public required Guid UserId { get; init; }
    
    /// <summary>
    /// Идентификатор OAuth 2.0 / OpenID Connect клиента, инициировавшего запрос авторизации
    /// </summary>
    public required string ClientId { get; init; }
    
    /// <summary>
    /// Массив запрошенных scope'ов аутентификации, определяющих уровень доступа
    /// </summary>
    public required ImmutableArray<string> Scopes { get; init; }
    
    /// <summary>
    /// Схема аутентификации, используемая для создания ClaimsPrincipal
    /// </summary>
    public required string AuthenticationScheme { get; init; }
    
    /// <summary>
    /// Исходный authenticated identity, содержащий базовые claims пользователя
    /// </summary>
    public required ClaimsIdentity Identity { get; init; }
}