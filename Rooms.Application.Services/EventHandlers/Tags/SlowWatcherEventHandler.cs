using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события изменения скорости воспроизведения зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class SlowWatcherEventHandler(IUnitOfWork unitOfWork)
    : BeforeSaveNotificationHandler<ViewerSpeedChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения скорости воспроизведения
    /// </summary>
    /// <param name="notification">Событие изменения скорости</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerSpeedChangedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Viewer.Speed < 1.0)
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.SlowWatcher);
        else
            notification.Room.RemoveTag(notification.Viewer.Id, Constants.ViewerTags.SlowWatcher);

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}