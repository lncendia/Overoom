using Rooms.Application.Abstractions.Events;

namespace Rooms.Infrastructure.Bus.Services;

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
    /// <param name="connectionId">Идентификатор подключения, которое следует исключить из получателей (например, отправитель события)</param>
    /// <returns>Задача, представляющая асинхронную операцию отправки</returns>
    Task SendAsync(RoomBaseEvent @event, Guid roomId, string? connectionId);
}