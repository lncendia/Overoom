using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события изменения аватара зрителя
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class NoAvatarEventHandler(IUnitOfWork unitOfWork) : BeforeSaveNotificationHandler<ViewerPhotoChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения аватара
    /// </summary>
    /// <param name="notification">Событие изменения аватара</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerPhotoChangedEvent notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(notification.Viewer.PhotoKey))
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.NoAvatar);
        else 
            notification.Room.RemoveTag(notification.Viewer.Id, Constants.ViewerTags.NoAvatar);

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}