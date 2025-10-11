using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события отправки бипа (звукового сигнала) зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class BeeperEventHandler(IUnitOfWork unitOfWork)
    : BeforeSaveNotificationHandler<ViewerBeepedEvent>
{
    /// <summary>
    /// Обрабатывает событие отправки бипа зрителем
    /// </summary>
    /// <param name="notification">Событие отправки бипа</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerBeepedEvent notification, CancellationToken cancellationToken)
    {
        var count = notification.Room.IncrementStatisticParameter(
            notification.Initiator.Id, Constants.ViewerStatisticParameters.BeepCount);

        if (count > 10)
            notification.Room.AddTag(notification.Initiator.Id, Constants.ViewerTags.Beeper);

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}