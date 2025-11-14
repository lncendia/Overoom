using Rooms.Application.Abstractions.RoomEvents;

namespace Rooms.Application.Abstractions.Services;

/// <summary>
/// Интерфейс отправителя событий комнаты
/// </summary>
public interface IRoomEventSender
{
    /// <summary>
    /// Отправляет событие комнаты всем клиентам в группе, кроме указанного подключения
    /// </summary>
    /// <param name="event">Событие комнаты для отправки</param>
    /// <param name="roomId">Идентификатор комнаты</param>
    /// <param name="excludedConnectionId">Идентификатор подключения, которое следует исключить из получателей (например, отправитель события)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Задача, представляющая асинхронную операцию отправки</returns>
    Task SendAsync(RoomBaseEvent @event, Guid roomId, string? excludedConnectionId,
        CancellationToken cancellationToken = default);
}