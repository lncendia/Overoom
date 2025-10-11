using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события изменения полноэкранного режима зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class FullscreenerEventHandler(IUnitOfWork unitOfWork) : BeforeSaveNotificationHandler<ViewerFullScreenChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения полноэкранного режима
    /// </summary>
    /// <param name="notification">Событие изменения полноэкранного режима</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerFullScreenChangedEvent notification, CancellationToken cancellationToken)
    {
        // Если это синхронизация - не обрабатываем
        if (notification.IsSyncEvent) return;

        if (notification.Viewer.FullScreen)
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.Fullscreener);
        else
            notification.Room.RemoveTag(notification.Viewer.Id, Constants.ViewerTags.Fullscreener);

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}