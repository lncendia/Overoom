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

    OpenIddictRequest? request = JsonSerializer.Deserialize<OpenIddictRequest>(json);
    if (request == null)
      return null;

    // Используем dummy-хост т.к. returnUrl может быть относительным путем
    var uri = new Uri("https://dummy" + returnUrl);
    Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(uri.Query);

    foreach ((string key, OpenIddictParameter openIddictParameter) in request.GetParameters())
    {
      string? value = openIddictParameter.ToString();
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
    if (sub == null && response.IsGranted)
      throw new ArgumentNullException(nameof(sub),
        @"User is not currently authenticated, and no subject id passed");

    string id = request.GetRequestId(sub);
    string key = string.Format(ConsentKeyFormat, id);
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
    var response = new ConsentResponse
    {
      GrantedScopes = [],
      RememberConsent = false,
      Description = null
    };

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
    string id = request.GetRequestId(sub);
    string key = string.Format(ConsentKeyFormat, id);
    string? json = session.GetString(key);
    if (json == null)
      return null;

    session.Remove(key);

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

    string normalizedScopes = request.GetScopes()
      .OrderBy(x => x, StringComparer.Ordinal)
      .Distinct(StringComparer.Ordinal)
      .Aggregate(string.Empty, (acc, s) => string.IsNullOrEmpty(acc) ? s : acc + "," + s);

    string value = $"{request.ClientId}:{sub}:{request.Nonce}:{normalizedScopes}";
    byte[] bytes = Encoding.UTF8.GetBytes(value);
    byte[] hash = SHA256.HashData(bytes);

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