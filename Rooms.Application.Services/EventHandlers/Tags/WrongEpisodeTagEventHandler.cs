using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события изменения серии зрителем для управления тегом "Неправильная серия"
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class WrongEpisodeTagEventHandler(IUnitOfWork unitOfWork)
    : BeforeSaveNotificationHandler<ViewerEpisodeChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения серии зрителем
    /// </summary>
    /// <param name="notification">Событие изменения серии зрителем</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerEpisodeChangedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Viewer.Season == notification.Room.Owner.Season && notification.Viewer.Episode == notification.Room.Owner.Episode)
        {
            notification.Room.RemoveTag(notification.Viewer.Id, Constants.ViewerTags.WrongEpisode);
        }
        else
        {
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.WrongEpisode);
        }

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}