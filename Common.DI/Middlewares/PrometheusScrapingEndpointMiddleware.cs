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
        // Получаем данные аутентификации из конфигурации
        var username = app.Configuration.GetRequiredValue<string>("OpenTelemetry:Username");
        var passwordHash = app.Configuration.GetRequiredValue<string>("OpenTelemetry:PasswordHash");

        app.Use(async (context, next) =>
        {
            // Пропускаем все запросы, кроме /metrics
            if (context.Request.Path != "/metrics")
            {
                await next();
                return;
            }

            // Проверяем заголовок Authorization
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (!IsAuthorized(authHeader, username, passwordHash))
            {
                // Возвращаем 401 с WWW-Authenticate, чтобы браузер показал окно логина
                context.Response.Headers.WWWAuthenticate = "Basic realm=\"Metrics\"";
                context.Response.StatusCode = 401;
                return;
            }

            // Всё ок — пропускаем дальше
            await next();
        });

        // Настраиваем эндпоинт Prometheus
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
        // Проверяем наличие заголовка
        if (string.IsNullOrEmpty(authHeader)) return false;

        // Парсим заголовок авторизации
        if (!AuthenticationHeaderValue.TryParse(authHeader, out var headerValue)) return false;

        // Проверяем что используется схема Basic Auth и есть параметры
        if (!headerValue.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrEmpty(headerValue.Parameter)) return false;

        // Декодируем credentials в формате username:password
        // Используем кодировку iso-8859-1 как указано в спецификации Basic Auth
        var decoded = Encoding.GetEncoding("iso-8859-1")
            .GetString(Convert.FromBase64String(headerValue.Parameter));

        // Ищем разделитель между именем пользователя и паролем
        var separatorIndex = decoded.IndexOf(':');
        if (separatorIndex < 0) return false;

        // Извлекаем имя пользователя и пароль из decoded строки
        var providedUsername = decoded[..separatorIndex];
        var providedPassword = decoded[(separatorIndex + 1)..];

        // Сравниваем с ожидаемыми значениями
        return providedUsername == username && PasswordHasher.Verify(providedPassword, passwordHash);
    }
}