using Common.Application.ScopedDictionary;
using Rooms.Application.Abstractions;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services;

/// <summary>
/// Вспомогательные методы расширения для работы с заголовками сообщений
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// Устанавливает заголовки для события комнаты с указанием идентификатора подключения
    /// </summary>
    /// <param name="dictionary">Контекст публикации сообщения</param>
    /// <param name="event">Событие комнаты</param>
    /// <param name="connectionId">Идентификатор подключения SignalR</param>
    public static void SetRoomHeaders(this IScopedDictionary dictionary, IRoomEvent @event, string connectionId)
    {
        // Установка заголовка с идентификатором подключения
        dictionary[Constants.ExcludedConnectionIdHeader] = connectionId;

        // Установка остальных заголовков комнаты
        dictionary.SetRoomHeaders(@event);
    }

    /// <summary>
    /// Устанавливает базовые заголовки для события комнаты
    /// </summary>
    /// <param name="dictionary">Контекст публикации сообщения</param>
    /// <param name="event">Событие комнаты</param>
    public static void SetRoomHeaders(this IScopedDictionary dictionary, IRoomEvent @event)
    {
        // Установка заголовка с идентификатором комнаты
        dictionary[Constants.RoomIdHeader] = @event.Room.Id;
    }

    /// <summary>
    /// Устанавливает заголовки для команды комнаты с указанием идентификатора подключения
    /// </summary>
    /// <param name="dictionary">Контекст публикации сообщения</param>
    /// <param name="roomId">Идентификатор комнаты</param>
    /// <param name="connectionId">Идентификатор подключения</param>
    public static void SetRoomHeaders(this IScopedDictionary dictionary, Guid roomId, string? connectionId = null)
    {
        // Установка заголовка с идентификатором подключения
        if (connectionId != null)
            dictionary[Constants.ExcludedConnectionIdHeader] = connectionId;

        // Установка заголовка с идентификатором комнаты
        dictionary[Constants.RoomIdHeader] = roomId;
    }
}