using System.Security.Claims;
using OpenIddict.Abstractions;

namespace Identix.Infrastructure.Web.Extensions;

/// <summary>
/// Расширения для работы с аутентификацией
/// </summary>
public static class CommonExtensions
{
    /// <summary>
    /// Возвращает идентификатор пользователя из объекта ClaimsPrincipal.
    /// </summary>
    /// <param name="principal">Объект ClaimsPrincipal.</param>
    /// <returns>Идентификатор пользователя.</returns>
    public static Guid Id(this ClaimsPrincipal principal) => Guid.Parse(principal.FindFirstValue(OpenIddictConstants.Claims.Subject)!);

    /// <summary>
    /// Возвращает идентификатор пользователя из объекта ClaimsPrincipal.
    /// </summary>
    /// <param name="principal">Объект ClaimsPrincipal.</param>
    /// <returns>Идентификатор пользователя.</returns>
    public static string? GetId(this ClaimsPrincipal? principal) => principal?.FindFirstValue(OpenIddictConstants.Claims.Subject);
    
    /// <summary>
    /// Безопасно добавляет claim в ClaimsIdentity, если значение не пустое
    /// </summary>
    /// <param name="identity">Экземпляр ClaimsIdentity для добавления claim</param>
    /// <param name="type">Тип claim (например, "email", "name")</param>
    /// <param name="value">Значение claim</param>
    public static void TryAddClaim(this ClaimsIdentity identity, string type, string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        identity.AddClaim(new Claim(type, value));
    }
}