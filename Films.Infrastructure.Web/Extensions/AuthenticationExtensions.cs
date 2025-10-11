using System.Security.Claims;

namespace Films.Infrastructure.Web.Extensions;

/// <summary>
/// Расширения для работы с объектами безопасности.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Получает идентификатор пользователя из объекта ClaimsPrincipal.
    /// </summary>
    /// <param name="user">Объект ClaimsPrincipal, представляющий пользователя.</param>
    /// <returns>Идентификатор пользователя в формате GUID.</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается, если не удалось найти значение для указанного типа утверждения.</exception>
    public static Guid GetId(this ClaimsPrincipal user)
    {
        // Получаем значение для указанного типа утверждения (ClaimTypes.NameIdentifier) из объекта ClaimsPrincipal.
        // Если значение не найдено, выбрасывается исключение InvalidOperationException.
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Преобразуем значение идентификатора в формат GUID.
        return Guid.Parse(id);
    }
}
