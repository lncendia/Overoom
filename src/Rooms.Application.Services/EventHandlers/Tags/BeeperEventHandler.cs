using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события отправки бипа (звукового сигнала) зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
/// <param name="statistics">Счётчики действий зрителей</param>
public class BeeperEventHandler(IUnitOfWork unitOfWork, IViewerStatistics statistics)
  : BeforeSaveNotificationHandler<ViewerBeepedEvent>
{
  /// <summary>
  /// Обрабатывает событие отправки бипа зрителем
  /// </summary>
  /// <param name="notification">Событие отправки бипа</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  protected override async Task Execute(ViewerBeepedEvent notification, CancellationToken cancellationToken)
  {
    int count = await statistics.IncrementAsync(notification.Room.Id, notification.Initiator.Id,
      Constants.ViewerStatisticParameters.BeepCount, cancellationToken);

    if (count > 10)
    {
      notification.Room.AddTag(notification.Initiator.Id, Constants.ViewerTags.Beeper);
      await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
  }
}