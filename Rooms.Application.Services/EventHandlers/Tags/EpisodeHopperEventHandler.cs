using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события изменения серии зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class EpisodeHopperEventHandler(IUnitOfWork unitOfWork)
    : BeforeSaveNotificationHandler<ViewerEpisodeChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения серии зрителем
    /// </summary>
    /// <param name="notification">Событие изменения серии</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerEpisodeChangedEvent notification, CancellationToken cancellationToken)
    {
        // Если это синхронизация - не обрабатываем
        if (notification.IsSyncEvent) return;
        
        var hops = notification.Room.IncrementStatisticParameter(notification.Viewer.Id,
            Constants.ViewerStatisticParameters.EpisodeChangeCount);
        if (hops > 30)
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.EpisodeHopper);

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}