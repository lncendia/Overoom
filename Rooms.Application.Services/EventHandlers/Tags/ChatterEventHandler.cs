using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Messages.Events;
using Rooms.Domain.Repositories;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события отправки сообщения в чате
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class ChatterEventHandler(IUnitOfWork unitOfWork)
    : BeforeSaveNotificationHandler<NewMessageEvent>
{
    /// <summary>
    /// Обрабатывает событие отправки нового сообщения
    /// </summary>
    /// <param name="notification">Событие нового сообщения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(NewMessageEvent notification, CancellationToken cancellationToken)
    {
        var count = notification.Room.IncrementStatisticParameter(notification.Viewer.Id, Constants.ViewerStatisticParameters.MessagesCount);
        if (count > 50)
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.Chatter);

        if (count > 100)
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.ChatterOverdrive);

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}