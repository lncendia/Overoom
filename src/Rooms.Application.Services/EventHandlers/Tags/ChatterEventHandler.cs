using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Messages.Events;
using Rooms.Domain.Repositories;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события отправки сообщения в чате
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
/// <param name="statistics">Счётчики действий зрителей</param>
public class ChatterEventHandler(IUnitOfWork unitOfWork, IViewerStatistics statistics)
  : BeforeSaveNotificationHandler<NewMessageEvent>
{
  /// <summary>
  /// Обрабатывает событие отправки нового сообщения
  /// </summary>
  /// <param name="notification">Событие нового сообщения</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  protected override async Task Execute(NewMessageEvent notification, CancellationToken cancellationToken)
  {
    int count = await statistics.IncrementAsync(notification.Room.Id, notification.Viewer.Id,
      Constants.ViewerStatisticParameters.MessagesCount, cancellationToken);
    if (count > 50)
    {
      notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.Chatter);
      await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }

    if (count > 100)
    {
      notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.ChatterOverdrive);
      await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
  }
}
