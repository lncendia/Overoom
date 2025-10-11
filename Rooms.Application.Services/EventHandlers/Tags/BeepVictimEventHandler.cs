using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события получения бипа (звукового сигнала) зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class BeepVictimEventHandler(IUnitOfWork unitOfWork)
    : BeforeSaveNotificationHandler<ViewerBeepedEvent>
{
    /// <summary>
    /// Обрабатывает событие получения бипа зрителем
    /// </summary>
    /// <param name="notification">Событие отправки бипа</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerBeepedEvent notification, CancellationToken cancellationToken)
    {
        var count = notification.Room.IncrementStatisticParameter(
            notification.Target.Id, Constants.ViewerStatisticParameters.BeepedCount);

        if (count > 20)
            notification.Room.AddTag(notification.Target.Id, Constants.ViewerTags.BeepVictim);

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}