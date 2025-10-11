using System.Security.Claims;
using MediatR;

namespace Identix.Application.Abstractions.Commands.OpenId;

/// <summary>
/// Команда для обновления ClaimsPrincipal с актуальными данными пользователя.
/// </summary>
public class RefreshPrincipalCommand : IRequest<ClaimsPrincipal>
{
    /// <summary>
    /// Уникальный идентификатор пользователя, для которого требуется обновить principal
    /// </summary>
    public required Guid UserId { get; init; }
    
    /// <summary>
    /// Текущий ClaimsIdentity, подлежащий обновлению
    /// </summary>
    public required ClaimsIdentity Identity { get; init; }
    
    /// <summary>
    /// Схема аутентификации, используемая для создания обновленного principal
    /// </summary>
    public required string AuthenticationScheme { get; init; }
}