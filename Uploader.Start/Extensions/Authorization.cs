using System.Security.Claims;

namespace Uploader.Start.Extensions;

/// <summary>
/// Статический класс, предоставляющий метод расширения для добавления авторизации по JWT в коллекцию сервисов.
/// </summary>
public static class Authorization
{
    /// <summary>
    /// Добавляет авторизацию по JWT в коллекцию сервисов.
    /// </summary>
    /// <param name="services">Коллекция служб.</param>
    public static void AddAuthorizationPolicies(this IServiceCollection services)
    {
        // Добавляет службы политики авторизации в указанную коллекцию IServiceCollection.
        services.AddAuthorizationBuilder()

            // Добавляет службы политики авторизации в указанную коллекцию IServiceCollection.
            .AddPolicy("admin", policy => { policy.RequireClaim(ClaimTypes.Role, "admin"); });
    }
}