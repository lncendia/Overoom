using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события отправки крика зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
/// <param name="statistics">Счётчики действий зрителей</param>
public class ScreamerEventHandler(IUnitOfWork unitOfWork, IViewerStatistics statistics)
  : BeforeSaveNotificationHandler<ViewerScreamedEvent>
{
  /// <summary>
  /// Обрабатывает событие отправки крика
  /// </summary>
  /// <param name="notification">Событие отправки крика</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  protected override async Task Execute(ViewerScreamedEvent notification, CancellationToken cancellationToken)
  {
    int count = await statistics.IncrementAsync(notification.Room.Id, notification.Initiator.Id,
      Constants.ViewerStatisticParameters.ScreamCount, cancellationToken);

    if (count > 5)
    {
      notification.Room.AddTag(notification.Initiator.Id, Constants.ViewerTags.Screamer);
      await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
  }
}
