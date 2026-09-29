using Common.Application.Events;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик исключения зрителя из комнаты, удаляющий его счётчики
/// </summary>
/// <param name="statistics">Счётчики действий зрителей</param>
public class RemoveKickedViewerStatisticsEventHandler(IViewerStatistics statistics)
  : AfterSaveNotificationHandler<ViewerKickedEvent>
{
  /// <summary>
  /// Обрабатывает событие исключения зрителя
  /// </summary>
  /// <param name="event">Событие исключения зрителя</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  protected override Task Execute(ViewerKickedEvent @event, CancellationToken cancellationToken)
  {
    return statistics.RemoveViewerAsync(@event.Room.Id, @event.Target.Id, cancellationToken);
  }
}
