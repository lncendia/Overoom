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
    var integrationEvent = new RoomViewerKickedIntegrationEvent
    {
      RoomId = notification.Room.Id,
      ViewerId = notification.ViewerId
    };

    await publishEndpoint.Publish(integrationEvent, cancellationToken: cancellationToken);
  }
}