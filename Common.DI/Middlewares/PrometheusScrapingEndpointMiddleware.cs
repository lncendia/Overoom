using System.Net.Http.Headers;
using System.Text;
using Common.DI.Extensions;
using Microsoft.AspNetCore.Builder;

namespace Common.DI.Middlewares;

/// <summary>
/// Расширение для добавления Basic Auth на эндпоинт /metrics.
/// </summary>
public static class PrometheusScrapingEndpointMiddleware
{
  /// <summary>
  /// Добавляет защиту Basic Authentication для эндпоинта Prometheus /metrics.
  /// </summary>
  /// <param name="app">Приложение WebApplication</param>
  public static void MapPrometheusScrapingEndpointWithBasicAuth(this WebApplication app)
  {
    string username = app.Configuration.GetRequiredValue<string>("OpenTelemetry:Username");
    string passwordHash = app.Configuration.GetRequiredValue<string>("OpenTelemetry:PasswordHash");

    app.Use(async (context, next) =>
    {
      if (context.Request.Path != "/metrics")
      {
        await next();
        return;
      }

      string? authHeader = context.Request.Headers.Authorization.FirstOrDefault();
      if (!IsAuthorized(authHeader, username, passwordHash))
      {
        context.Response.Headers.WWWAuthenticate = "Basic realm=\"Metrics\"";
        context.Response.StatusCode = 401;
        return;
      }

      await next();
    });

    app.MapPrometheusScrapingEndpoint("/metrics");
  }

  /// <summary>
  /// Проверка Basic Auth заголовка.
  /// </summary>
  /// <param name="authHeader">Значение заголовка Authorization из HTTP-запроса</param>
  /// <param name="username">Ожидаемое имя пользователя для проверки</param>
  /// <param name="passwordHash">Ожидаемый хеш пароля для проверки</param>
  /// <returns>true - если авторизация успешна, false - в противном случае</returns>
  private static bool IsAuthorized(string? authHeader, string username, string passwordHash)
  {
    if (string.IsNullOrEmpty(authHeader)) return false;
    if (!AuthenticationHeaderValue.TryParse(authHeader, out AuthenticationHeaderValue? headerValue)) return false;

    if (!headerValue.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase) ||
        string.IsNullOrEmpty(headerValue.Parameter)) return false;

    // Используем кодировку iso-8859-1 как указано в спецификации Basic Auth
    string decoded = Encoding.GetEncoding("iso-8859-1")
      .GetString(Convert.FromBase64String(headerValue.Parameter));

    int separatorIndex = decoded.IndexOf(':');
    if (separatorIndex < 0) return false;

    string providedUsername = decoded[..separatorIndex];
    string providedPassword = decoded[(separatorIndex + 1)..];

    return providedUsername == username && PasswordHasher.Verify(providedPassword, passwordHash);
  }
}