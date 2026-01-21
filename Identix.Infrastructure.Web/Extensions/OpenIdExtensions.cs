using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;

namespace Identix.Infrastructure.Web.Extensions;

/// <summary>
/// Методы расширения для работы с OpenID Connect
/// </summary>
public static class OpenIdExtensions
{
  /// <summary>
  /// Ключ для хранения OIDC запроса в сессии
  /// </summary>
  private const string OpenIdRequestKey = "OpenIdRequest";

  /// <summary>
  /// Формат ключа для хранения согласия (consent) в сессии
  /// </summary>
  private const string ConsentKeyFormat = "Consent_{0}";

  /// <summary>
  /// Сохраняет OIDC запрос в сессии
  /// </summary>
  /// <param name="session">Сессия</param>
  /// <param name="request">OIDC запрос</param>
  public static void SetOpenIdRequest(this ISession session, OpenIddictRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    string json = JsonSerializer.Serialize(request);
    session.SetString(OpenIdRequestKey, json);
  }

  /// <summary>
  /// Получает OIDC запрос из сессии и проверяет его соответствие returnUrl
  /// </summary>
  /// <param name="session">Сессия</param>
  /// <param name="returnUrl">URL для возврата</param>
  /// <returns>OIDC запрос или null если не найден или не соответствует</returns>
  public static OpenIddictRequest? GetOpenIdRequest(this ISession session, string returnUrl)
  {
    string? json = session.GetString(OpenIdRequestKey);
    if (json == null)
      return null;

    // Десериализуем запрос из JSON
    OpenIddictRequest? request = JsonSerializer.Deserialize<OpenIddictRequest>(json);
    if (request == null)
      return null;

    // Парсим query string из returnUrl для проверки соответствия параметров
    // Используем dummy-хост т.к. returnUrl может быть относительным путем
    var uri = new Uri("https://dummy" + returnUrl);
    Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(uri.Query);

    // Сравниваем все параметры из оригинального запроса с параметрами в returnUrl
    foreach ((string key, OpenIddictParameter openIddictParameter) in request.GetParameters())
    {
      string? value = openIddictParameter.ToString();
      // Если параметр отсутствует в returnUrl или значения не совпадают - возвращаем null
      if (!query.TryGetValue(key, out StringValues qValue) || qValue != value)
        return null;
    }

    return request;
  }

  /// <summary>
  /// Сохраняет согласие пользователя на выдачу scope'ов в сессии
  /// </summary>
  /// <param name="session">Сессия пользователя</param>
  /// <param name="request">OIDC-запрос авторизации</param>
  /// <param name="sub">Идентификатор пользователя (subject)</param>
  /// <param name="response">Ответ согласия (какие scope разрешены, нужно ли запоминать)</param>
  /// <exception cref="ArgumentNullException">
  /// Если пользователь не аутентифицирован, но пытаются выдать scope'ы
  /// </exception>
  public static void GrantConsent(this ISession session, OpenIddictRequest request, string? sub,
    ConsentResponse response)
  {
    // Проверка: если пользователь не аутентифицирован, нельзя выдавать scope'ы
    if (sub == null && response.IsGranted)
      throw new ArgumentNullException(nameof(sub),
        @"User is not currently authenticated, and no subject id passed");

    // Генерируем уникальный ключ для хранения согласия в сессии
    string id = request.GetRequestId(sub);
    string key = string.Format(ConsentKeyFormat, id);

    // Сериализуем объект ConsentResponse в JSON и сохраняем в сессии
    string json = JsonSerializer.Serialize(response);
    session.SetString(key, json);
  }

  /// <summary>
  /// Отмечает согласие пользователя как отклонённое (явный отказ)
  /// </summary>
  /// <param name="session">Сессия пользователя</param>
  /// <param name="request">OIDC-запрос авторизации</param>
  /// <param name="sub">Идентификатор пользователя (subject)</param>
  public static void DenyConsent(this ISession session, OpenIddictRequest request, string? sub)
  {
    // Создаем объект ConsentResponse с пустым набором scope'ов
    // Это означает явный отказ пользователя
    var response = new ConsentResponse
    {
      GrantedScopes = [],
      RememberConsent = false,
      Description = null
    };

    // Сохраняем отказ в сессии через GrantConsent
    session.GrantConsent(request, sub, response);
  }

  /// <summary>
  /// Извлекает сохранённое согласие пользователя из сессии и удаляет его.
  /// </summary>
  /// <param name="session">HTTP-сессия.</param>
  /// <param name="request">OIDC-запрос.</param>
  /// <param name="sub">Идентификатор пользователя (subject).</param>
  /// <returns>Объект согласия или <c>null</c>, если оно не найдено.</returns>
  public static ConsentResponse? TakeConsent(this ISession session, OpenIddictRequest request, string? sub)
  {
    // Формируем ключ для доступа к согласиям
    string id = request.GetRequestId(sub);
    string key = string.Format(ConsentKeyFormat, id);

    // Пытаемся получить JSON из сессии
    string? json = session.GetString(key);
    if (json == null)
      return null;

    // Удаляем согласие из сессии после чтения
    session.Remove(key);

    // Десериализуем и возвращаем согласие
    return JsonSerializer.Deserialize<ConsentResponse>(json);
  }

  /// <summary>
  /// Генерирует уникальный идентификатор запроса на основе его параметров
  /// </summary>
  /// <param name="request">OIDC запрос</param>
  /// <param name="sub">Идентификатор пользователя (опционально)</param>
  /// <returns>Уникальный хэш запроса</returns>
  private static string GetRequestId(this OpenIddictRequest request, string? sub = null)
  {
    ArgumentNullException.ThrowIfNull(request);

    // Нормализуем scope'ы: сортируем, удаляем дубликаты и объединяем в строку
    string normalizedScopes = request.GetScopes()
      .OrderBy(x => x, StringComparer.Ordinal)
      .Distinct(StringComparer.Ordinal)
      .Aggregate(string.Empty, (acc, s) => string.IsNullOrEmpty(acc) ? s : acc + "," + s);

    // Создаем строку для хэширования из ключевых параметров запроса
    string value = $"{request.ClientId}:{sub}:{request.Nonce}:{normalizedScopes}";
    byte[] bytes = Encoding.UTF8.GetBytes(value);

    // Хэшируем с помощью SHA256 для создания уникального идентификатора
    byte[] hash = SHA256.HashData(bytes);

    // Кодируем в URL-safe Base64
    return Base64UrlEncoder.Encode(hash);
  }

  /// <summary>
  /// Представляет ответ с согласием пользователя на предоставление доступа
  /// </summary>
  public class ConsentResponse
  {
    /// <summary>
    /// Список разрешенных scope'ов доступа
    /// </summary>
    public required IReadOnlyList<string> GrantedScopes { get; init; }

    /// <summary>
    /// Флаг сохранения согласия для будущих запросов
    /// </summary>
    public required bool RememberConsent { get; init; }

    /// <summary>
    /// Дополнительное описание или комментарий к согласию
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Признак того, что пользователь разрешил хотя бы один scope
    /// </summary>
    public bool IsGranted => GrantedScopes.Any();
  }
}