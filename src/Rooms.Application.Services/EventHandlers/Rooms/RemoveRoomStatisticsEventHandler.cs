using Common.Application.Events;
using Common.Domain.Events;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события удаления комнаты, удаляющий счётчики её зрителей
/// </summary>
/// <param name="statistics">Счётчики действий зрителей</param>
public class RemoveRoomStatisticsEventHandler(IViewerStatistics statistics)
  : AfterSaveNotificationHandler<DeleteEvent<Room>>
{
  /// <summary>
  /// Обрабатывает событие удаления комнаты
  /// </summary>
  /// <param name="event">Событие удаления комнаты</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  protected override Task Execute(DeleteEvent<Room> @event, CancellationToken cancellationToken)
  {
    return statistics.RemoveRoomAsync(@event.Id, cancellationToken);
  }
}
