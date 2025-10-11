using Common.Application.Events;
using Common.IntegrationEvents.Rooms;
using Films.Domain.Rooms.Events;
using MassTransit;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Обработчик доменного события исключения зрителя из комнаты
/// </summary>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
public class ViewerKickedEventHandler(IPublishEndpoint publishEndpoint)
    : BeforeSaveNotificationHandler<ViewerKickedEvent>
{
    /// <summary>
    /// Обрабатывает событие исключения зрителя и публикует интеграционное событие
    /// </summary>
    /// <param name="notification">Доменное событие исключения зрителя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerKickedEvent notification, CancellationToken cancellationToken)
    {
        // Создаем событие интеграции для оповещения других сервисов об исключении зрителя
        var integrationEvent = new RoomViewerKickedIntegrationEvent
        {
            RoomId = notification.Room.Id,
            ViewerId = notification.ViewerId
        };

        // Публикуем событие интеграции через MassTransit
        await publishEndpoint.Publish(integrationEvent, cancellationToken: cancellationToken);
    }
}