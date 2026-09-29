using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события изменения серии зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
/// <param name="statistics">Счётчики действий зрителей</param>
public class EpisodeHopperEventHandler(IUnitOfWork unitOfWork, IViewerStatistics statistics)
  : BeforeSaveNotificationHandler<ViewerEpisodeChangedEvent>
{
  /// <summary>
  /// Обрабатывает событие изменения серии зрителем
  /// </summary>
  /// <param name="notification">Событие изменения серии</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  protected override async Task Execute(ViewerEpisodeChangedEvent notification, CancellationToken cancellationToken)
  {
    if (notification.IsSyncEvent) return;

    int hops = await statistics.IncrementAsync(notification.Room.Id, notification.Viewer.Id,
      Constants.ViewerStatisticParameters.EpisodeChangeCount, cancellationToken);
    if (hops > 30)
    {
      notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.EpisodeHopper);
      await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
  }
}
