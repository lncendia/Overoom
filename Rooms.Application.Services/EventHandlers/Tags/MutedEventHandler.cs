using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события изменения состояния звука зрителя
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class MutedEventHandler(IUnitOfWork unitOfWork) : BeforeSaveNotificationHandler<ViewerMuteChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения состояния звука
    /// </summary>
    /// <param name="notification">Событие изменения состояния звука</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerMuteChangedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Viewer.Muted)
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.Muted);
        else
            notification.Room.RemoveTag(notification.Viewer.Id, Constants.ViewerTags.Muted);

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}