using Common.Application.Events;
using Common.Domain.Events;
using Common.IntegrationEvents.Rooms;
using Films.Domain.Rooms;
using MassTransit;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Обработчик доменного события удаления комнаты
/// </summary>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
public class RoomDeletedEventHandler(IPublishEndpoint publishEndpoint) : BeforeSaveNotificationHandler<DeleteEvent<Room>>
{
    /// <summary>
    /// Обрабатывает событие удаления комнаты и публикует интеграционное событие
    /// </summary>
    /// <param name="notification">Доменное событие удаления комнаты</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(DeleteEvent<Room> notification, CancellationToken cancellationToken)
    {
        // Создаем событие интеграции для оповещения других сервисов об удалении комнаты
        var integrationEvent = new RoomDeletedIntegrationEvent
        {
            Id = notification.Id
        };
        
        // Публикуем событие интеграции через MassTransit
        await publishEndpoint.Publish(integrationEvent, cancellationToken: cancellationToken);
    }
}