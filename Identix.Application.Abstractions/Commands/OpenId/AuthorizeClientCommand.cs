using System.Collections.Immutable;
using System.Security.Claims;
using MediatR;

namespace Identix.Application.Abstractions.Commands.OpenId;

/// <summary>
/// Команда авторизации клиентского приложения в OpenID Connect
/// Используется для создания ClaimsPrincipal в клиентских flow (Client Credentials)
/// </summary>
public sealed record AuthorizeClientCommand : IRequest<ClaimsPrincipal>
{
    /// <summary>
    /// Идентификатор клиентского приложения (ClientId)
    /// </summary>
    public required string ClientId { get; init; }
    
    /// <summary>
    /// Запрашиваемые scope'ы доступа для клиентского приложения
    /// </summary>
    public required ImmutableArray<string> Scopes { get; init; }
}