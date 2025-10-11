using Microsoft.AspNetCore.Identity;

namespace Identix.Infrastructure.Web.Account.Services;

/// <summary>
/// Реализует callback для события OnRefreshingPrincipal у SecurityStampValidator.
/// </summary>
public static class SecurityStampValidatorCallback
{
    /// <summary>
    /// Сохраняет клеймы, добавленные при логине, которые не создаются ASP.NET Identity.
    /// Это нужно для того, чтобы не потерять такие клеймы, как idp, auth_time, amr.
    /// </summary>
    /// <param name="context">Контекст обновления principal.</param>
    /// <returns>Задача, представляющая асинхронную операцию</returns>
    /// <remarks>
    /// Метод предотвращает потерю важных OpenID Connect claims при обновлении security stamp.
    /// ASP.NET Identity по умолчанию заменяет весь principal, что приводит к потере
    /// специфичных OIDC claims, добавленных во время аутентификации.
    /// </remarks>
    public static Task UpdatePrincipal(SecurityStampRefreshingPrincipalContext context)
    {
        // Получаем типы claims из нового principal
        var newClaimTypes = context.NewPrincipal!.Claims.Select(x => x.Type).ToArray();
    
        // Находим claims в текущем principal, которые отсутствуют в новом
        var currentClaimsToKeep = context.CurrentPrincipal!.Claims
            .Where(x => !newClaimTypes.Contains(x.Type))
            .ToArray();

        // Добавляем сохраненные claims в новый principal
        var identity = context.NewPrincipal.Identities.First();
        identity.AddClaims(currentClaimsToKeep);

        return Task.CompletedTask;
    }
}
