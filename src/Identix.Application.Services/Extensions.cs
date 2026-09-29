using System.Collections.Specialized;
using Identix.Application.Abstractions.Entities;

namespace Identix.Application.Services;

/// <summary>
/// Класс с расширениями.
/// </summary>
public static class Extensions
{
  /// <summary>
  /// Генерирует URL для подтверждения регистрации по электронной почте.
  /// </summary>
  /// <param name="user">Пользователь.</param>
  /// <param name="url">Базовый URL.</param>
  /// <param name="code">Код подтверждения.</param>
  /// <param name="returnUrl">URL возврата.</param>
  /// <param name="query">Дополнительные параметры ссылки.</param>
  /// <returns>Сгенерированный URL.</returns>
  public static string GenerateMailConfirmUrl(this AppUser user, string url, string code, string? returnUrl,
    params KeyValuePair<string, object>[] query)
  {
    var uriBuilder = new UriBuilder(url);
    NameValueCollection queryParameters = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
    queryParameters["id"] = user.Id.ToString();
    queryParameters["code"] = code;

    if (returnUrl != null)
      queryParameters["returnUrl"] = returnUrl;

    foreach (KeyValuePair<string, object> keyValuePair in query)
    {
      queryParameters[keyValuePair.Key] = keyValuePair.Value.ToString();
    }

    uriBuilder.Query = queryParameters.ToString();

    return uriBuilder.ToString();
  }
}