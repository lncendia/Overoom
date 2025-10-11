using System.Collections.Immutable;
using System.Security.Claims;
using MediatR;

namespace Identix.Application.Abstractions.Commands.OpenId;

/// <summary>
/// Команда для предоставления согласия (consent) пользователя на доступ к запрашиваемым scope'ам OAuth 2.0 / OpenID Connect.
/// </summary>
public class GrantConsentCommand : IRequest<ClaimsPrincipal>
{
    /// <summary>
    /// Уникальный идентификатор пользователя, предоставляющего согласие
    /// </summary>
    public required Guid UserId { get; init; }
    
    /// <summary>
    /// Идентификатор клиентского приложения, запрашивающего доступ
    /// </summary>
    public required string ClientId { get; init; }
    
    /// <summary>
    /// Флаг, указывающий следует ли сохранить согласие для будущих авторизаций
    /// При значении true согласие сохраняется и не запрашивается повторно
    /// </summary>
    public required bool RememberConsent { get; init; }
    
    /// <summary>
    /// Список scope'ов, на которые пользователь предоставляет согласие
    /// </summary>
    public required ImmutableArray<string> Scopes { get; init; }
    
    /// <summary>
    /// Схема аутентификации, используемая для создания ClaimsPrincipal
    /// </summary>
    public required string AuthenticationScheme { get; init; }
    
    /// <summary>
    /// Дополнительное описание или комментарий к согласию
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// Текущий authenticated identity пользователя, содержащий базовые claims
    /// </summary>
    public required ClaimsIdentity Identity { get; init; }
}