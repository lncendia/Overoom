using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события изменения состояния паузы зрителем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class PauseMasterEventHandler(IUnitOfWork unitOfWork) : BeforeSaveNotificationHandler<ViewerPauseChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения состояния паузы
    /// </summary>
    /// <param name="notification">Событие изменения состояния паузы</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerPauseChangedEvent notification, CancellationToken cancellationToken)
    {
        // Если это синхронизация или буферизация - не обрабатываем
        if (notification.IsSyncEvent || notification.Buffering) return;

        if (!notification.Viewer.OnPause) return;
        var pauses =
            notification.Room.IncrementStatisticParameter(notification.Viewer.Id, Constants.ViewerStatisticParameters.PauseCount);
        if (pauses > 30)
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.PauseMaster);

        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}