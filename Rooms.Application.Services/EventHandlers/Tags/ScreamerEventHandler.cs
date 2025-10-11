using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события отправки крика зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class ScreamerEventHandler(IUnitOfWork unitOfWork)
    : BeforeSaveNotificationHandler<ViewerScreamedEvent>
{
    /// <summary>
    /// Обрабатывает событие отправки крика
    /// </summary>
    /// <param name="notification">Событие отправки крика</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerScreamedEvent notification, CancellationToken cancellationToken)
    {
        var count = notification.Room.IncrementStatisticParameter(
            notification.Initiator.Id, Constants.ViewerStatisticParameters.ScreamCount);

        if (count > 5)
            notification.Room.AddTag(notification.Initiator.Id, Constants.ViewerTags.Screamer);

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}