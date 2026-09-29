using Common.Application.Events;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик выхода зрителя из комнаты, удаляющий его счётчики
/// </summary>
/// <param name="statistics">Счётчики действий зрителей</param>
public class RemoveLeavedViewerStatisticsEventHandler(IViewerStatistics statistics)
  : AfterSaveNotificationHandler<ViewerLeavedEvent>
{
  /// <summary>
  /// Обрабатывает событие выхода зрителя
  /// </summary>
  /// <param name="event">Событие выхода зрителя</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  protected override Task Execute(ViewerLeavedEvent @event, CancellationToken cancellationToken)
  {
    return statistics.RemoveViewerAsync(@event.Room.Id, @event.Viewer.Id, cancellationToken);
  }
}
