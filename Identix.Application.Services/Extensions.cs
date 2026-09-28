using System.Collections.Specialized;
using System.Reflection;
using Identix.Application.Abstractions.Entities;
using MassTransit;
using MassTransit.Middleware.Outbox;

namespace Identix.Application.Services;

/// <summary>
/// Класс с расширениями.
/// </summary>
public static class Extensions
{
  /// <summary>
  /// Отключает Outbox для указанного <see cref="IPublishEndpoint"/>
  /// </summary>
  /// <param name="publishEndpoint">Экземпляр <see cref="IPublishEndpoint"/>, для которого нужно пропустить Outbox.</param>
  public static IPublishEndpoint SkipOutbox(this IPublishEndpoint publishEndpoint)
  {
    // Ищем приватное свойство "PublishEndpointProvider" у текущего publishEndpoint
    // (используется внутри MassTransit для управления Outbox)
    PropertyInfo? property = publishEndpoint.GetType()
      .GetProperty("PublishEndpointProvider", BindingFlags.Instance | BindingFlags.NonPublic);

    if (property == null)
      return publishEndpoint;

    object? value = property.GetValue(publishEndpoint);

    if (value is not OutboxPublishEndpointProvider outboxPublishEndpointProvider)
      return publishEndpoint;

    // У OutboxPublishEndpointProvider ищем приватное поле "_publishEndpointProvider"
    // — оно содержит оригинальный провайдер, минуя Outbox.
    FieldInfo? innerProvider = outboxPublishEndpointProvider.GetType()
      .GetField("_publishEndpointProvider", BindingFlags.Instance | BindingFlags.NonPublic);

    if (innerProvider == null)
      return publishEndpoint;

    property.SetValue(publishEndpoint, innerProvider.GetValue(outboxPublishEndpointProvider));

    return publishEndpoint;
  }

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