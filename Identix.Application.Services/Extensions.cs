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
        var property = publishEndpoint.GetType()
            .GetProperty("PublishEndpointProvider", BindingFlags.Instance | BindingFlags.NonPublic);

        // Если свойство отсутствует — вернуть без изменений
        if (property == null)
            return publishEndpoint;

        // Извлекаем значение свойства (провайдера)
        var value = property.GetValue(publishEndpoint);

        // Проверяем, является ли оно OutboxPublishEndpointProvider
        if (value is not OutboxPublishEndpointProvider outboxPublishEndpointProvider)
            return publishEndpoint;

        // У OutboxPublishEndpointProvider ищем приватное поле "_publishEndpointProvider"
        // — оно содержит оригинальный провайдер, минуя Outbox.
        var innerProvider = outboxPublishEndpointProvider.GetType()
            .GetField("_publishEndpointProvider", BindingFlags.Instance | BindingFlags.NonPublic);

        // Если не нашли внутренний провайдер — вернуть без изменений
        if (innerProvider == null)
            return publishEndpoint;

        // Подменяем текущий PublishEndpointProvider на исходный (вне Outbox)
        property.SetValue(publishEndpoint, innerProvider.GetValue(outboxPublishEndpointProvider));

        // Возвращаем обновлённый экземпляр
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
        // Создаем объект UriBuilder с базовым URL
        var uriBuilder = new UriBuilder(url);

        // Получаем коллекцию параметров запроса
        var queryParameters = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

        // Добавляем параметр "id" со значением
        queryParameters["id"] = user.Id.ToString();

        // Добавляем параметр "code" со значением
        queryParameters["code"] = code;
        
        // Добавляем параметр "returnUrl" со значением
        if (returnUrl != null)
            queryParameters["returnUrl"] = returnUrl;

        // Добавляем дополнительные параметры
        foreach (var keyValuePair in query)
        {
            queryParameters[keyValuePair.Key] = keyValuePair.Value.ToString();
        }

        // Устанавливаем обновленную строку запроса
        uriBuilder.Query = queryParameters.ToString();

        // Получаем обновленный URL
        return uriBuilder.ToString();
    }
}