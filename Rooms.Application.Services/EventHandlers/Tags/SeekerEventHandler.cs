using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события изменения временной позиции воспроизведения зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class SeekerEventHandler(IUnitOfWork unitOfWork)
    : BeforeSaveNotificationHandler<ViewerTimeLineChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения временной позиции
    /// </summary>
    /// <param name="notification">Событие изменения временной позиции</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerTimeLineChangedEvent notification, CancellationToken cancellationToken)
    {
        // Если это синхронизация - не обрабатываем
        if (notification.IsSyncEvent) return;
        
        var seekCount =
            notification.Room.IncrementStatisticParameter(notification.Viewer.Id, Constants.ViewerStatisticParameters.SeekCount);

        if (seekCount > 20)
        {
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.Seeker);
        }

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}