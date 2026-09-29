using Common.Application.Events;
using Common.IntegrationEvents.Rooms;
using Films.Application.Abstractions;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using Films.Domain.Rooms.Events;
using Films.Domain.Rooms.Specifications;
using Films.Domain.Users;

using MassTransit;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Обработчик доменного события подключения зрителя к комнате
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
public class ViewerJoinedEventHandler(IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint)
  : BeforeSaveNotificationHandler<ViewerJoinedEvent>
{
  /// <summary>
  /// Обрабатывает событие подключения зрителя и публикует интеграционное событие
  /// </summary>
  /// <param name="notification">Доменное событие подключения зрителя</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  protected override async Task Execute(ViewerJoinedEvent notification, CancellationToken cancellationToken)
  {
    User user = notification.Viewer;
    var roomsSpecification = new RoomByUserSpecification(user.Id);
    int roomsCount = await unitOfWork.RoomRepository.Value.CountAsync(roomsSpecification, cancellationToken);
    if (roomsCount >= Constants.Limits.MaxRoomsPerUser) throw new MaxNumberRoomsReachedException(user.Id);

    var integrationEvent = new RoomViewerJoinedIntegrationEvent
    {
      RoomId = notification.Room.Id,
      Viewer = new Viewer
      {
        Id = user.Id,
        PhotoKey = user.PhotoKey,
        UserName = user.Username,
        Settings = user.RoomSettings.ToContract()
      }
    };

    await publishEndpoint.Publish(integrationEvent, cancellationToken: cancellationToken);
  }
}
