using System.Security.Claims;

namespace Films.Start.Extensions;

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
    services.AddAuthorizationBuilder()
      .AddPolicy("admin", policy => { policy.RequireClaim(ClaimTypes.Role, "admin"); });
  }
}
