using Common.Application.Events;
using Common.IntegrationEvents.Rooms;
using Films.Domain.Rooms.Events;
using MassTransit;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Обработчик доменного события выхода зрителя из комнаты
/// </summary>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
public class ViewerLeavedEventHandler(IPublishEndpoint publishEndpoint)
  : BeforeSaveNotificationHandler<ViewerLeavedEvent>
{
  /// <summary>
  /// Обрабатывает событие выхода зрителя и публикует интеграционное событие
  /// </summary>
  /// <param name="notification">Доменное событие выхода зрителя</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  protected override async Task Execute(ViewerLeavedEvent notification, CancellationToken cancellationToken)
  {
    var integrationEvent = new RoomViewerLeavedIntegrationEvent
    {
      RoomId = notification.Room.Id,
      ViewerId = notification.ViewerId
    };

    await publishEndpoint.Publish(integrationEvent, cancellationToken: cancellationToken);
  }
}