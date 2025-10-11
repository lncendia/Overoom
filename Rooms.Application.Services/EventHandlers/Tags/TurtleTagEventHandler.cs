using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события изменения временной позиции воспроизведения зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class TurtleTagEventHandler(IUnitOfWork unitOfWork)
    : BeforeSaveNotificationHandler<ViewerTimeLineChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения временной позиции
    /// </summary>
    /// <param name="notification">Событие изменения временной позиции</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerTimeLineChangedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Viewer.Season == notification.Room.Owner.Season &&
            notification.Viewer.Episode == notification.Room.Owner.Episode &&
            (notification.Room.Owner.TimeLine - notification.Viewer.TimeLine).TotalMinutes >= 5)
        {
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.Turtle);
        }
        else
        {
            notification.Room.RemoveTag(notification.Viewer.Id, Constants.ViewerTags.Turtle);
        }

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}