using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события отключения зрителя от комнаты
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
/// <param name="statistics">Счётчики действий зрителей</param>
public class LeaverEventHandler(IUnitOfWork unitOfWork, IViewerStatistics statistics)
  : BeforeSaveNotificationHandler<ViewerOnlineChangedEvent>
{
  /// <summary>
  /// Обрабатывает событие изменения онлайн-статуса зрителя
  /// </summary>
  /// <param name="notification">Событие изменения онлайн-статуса</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  protected override async Task Execute(ViewerOnlineChangedEvent notification, CancellationToken cancellationToken)
  {
    if (notification.Viewer.Online) return;

    int count = await statistics.IncrementAsync(notification.Room.Id, notification.Viewer.Id,
      Constants.ViewerStatisticParameters.DisconnectCount, cancellationToken);

    if (count > 5)
    {
      notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.Leaver);
      await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
  }
}