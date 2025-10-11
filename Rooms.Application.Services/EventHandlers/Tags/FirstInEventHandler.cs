using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события подключения зрителя к комнате
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class FirstInEventHandler(IUnitOfWork unitOfWork)
    : BeforeSaveNotificationHandler<ViewerJoinedEvent>
{
    /// <summary>
    /// Обрабатывает событие подключения зрителя
    /// </summary>
    /// <param name="notification">Событие подключения зрителя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerJoinedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Room.Viewers.Count == 2)
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.FirstIn);

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}