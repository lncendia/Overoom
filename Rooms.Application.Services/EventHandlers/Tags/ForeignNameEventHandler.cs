using Common.Application.Events;
using Rooms.Application.Abstractions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Tags;

/// <summary>
/// Обработчик события изменения имени зрителя
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class ForeignNameEventHandler(IUnitOfWork unitOfWork)
    : BeforeSaveNotificationHandler<ViewerNameChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения имени зрителя
    /// </summary>
    /// <param name="notification">Событие изменения имени</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerNameChangedEvent notification, CancellationToken cancellationToken)
    {
        // Проверка на иноагента: все буквы латинские (пробелы, спецсимволы игнорируются)
        var isForeignAgent = notification.Viewer.UserName
            .Where(char.IsLetter)
            .All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z');
        
        if (isForeignAgent)
        {
            notification.Room.AddTag(notification.Viewer.Id, Constants.ViewerTags.ForeignName);
        }
        else
        {
            notification.Room.RemoveTag(notification.Viewer.Id, Constants.ViewerTags.ForeignName);
        }
        
        await unitOfWork.RoomRepository.Value.UpdateAsync(notification.Room, cancellationToken);
    }
}