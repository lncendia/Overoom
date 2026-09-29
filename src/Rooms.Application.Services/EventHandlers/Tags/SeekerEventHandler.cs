using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события изменения временной позиции воспроизведения зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
/// <param name="statistics">Счётчики действий зрителей</param>
public class SeekerEventHandler(IUnitOfWork unitOfWork, IViewerStatistics statistics)
  : BeforeSaveNotificationHandler<ViewerTimeLineChangedEvent>
{
  /// <summary>
  /// Обрабатывает событие изменения временной позиции
  /// </summary>
  /// <param name="notification">Событие изменения временной позиции</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  protected override async Task Execute(ViewerTimeLineChangedEvent notification, CancellationToken cancellationToken)
  {
    if (notification.IsSyncEvent) return;

    int seekCount = await statistics.IncrementAsync(notification.Room.Id, notification.Viewer.Id,
      Constants.ViewerStatisticParameters.SeekCount, cancellationToken);

    if (seekCount > 20)
    {
      notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.Seeker);
      await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
  }
}
