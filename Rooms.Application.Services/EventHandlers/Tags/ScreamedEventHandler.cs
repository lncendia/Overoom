using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события получения крика зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class ScreamedEventHandler(IUnitOfWork unitOfWork)
    : BeforeSaveNotificationHandler<ViewerScreamedEvent>
{
    /// <summary>
    /// Обрабатывает событие получения крика
    /// </summary>
    /// <param name="notification">Событие отправки крика</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerScreamedEvent notification, CancellationToken cancellationToken)
    {
        var count = notification.Room.IncrementStatisticParameter(
            notification.Target.Id, Constants.ViewerStatisticParameters.ScreamedCount);

        if (count > 10)
            notification.Room.AddTag(notification.Target.Id, Constants.ViewerTags.Screamed);

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}