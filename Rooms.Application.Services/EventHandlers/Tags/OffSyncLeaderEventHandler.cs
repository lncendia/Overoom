using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события изменения состояния паузы зрителя
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class OffSyncLeaderEventHandler(IUnitOfWork unitOfWork)
    : BeforeSaveNotificationHandler<ViewerPauseChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения состояния паузы
    /// </summary>
    /// <param name="notification">Событие изменения состояния паузы</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerPauseChangedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Room.Owner.OnPause && !notification.Viewer.OnPause)
        {
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.OffSyncLeader);
        }
        else
        {
            notification.Room.RemoveTag(notification.Viewer.Id, Constants.ViewerTags.OffSyncLeader);
        }

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}