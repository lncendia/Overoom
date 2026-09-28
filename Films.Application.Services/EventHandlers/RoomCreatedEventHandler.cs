using Common.Application.Events;
using Common.IntegrationEvents.Rooms;
using Films.Application.Abstractions;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Films;
using Films.Domain.Repositories;
using Films.Domain.Rooms;
using Films.Domain.Rooms.Events;
using Films.Domain.Rooms.Specifications;
using Films.Domain.Users;

using MassTransit;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Обработчик доменного события создания комнаты
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
public class RoomCreatedEventHandler(IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint)
  : BeforeSaveNotificationHandler<RoomCreatedEvent>
{
  /// <summary>
  /// Обрабатывает событие создания комнаты и публикует интеграционное событие
  /// </summary>
  /// <param name="notification">Доменное событие создания комнаты</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  protected override async Task Execute(RoomCreatedEvent notification, CancellationToken cancellationToken)
  {
    Room room = notification.Room;
    User user = notification.Owner;
    Film film = notification.Film;
    var roomsSpecification = new RoomByUserSpecification(user.Id);
    int roomsCount = await unitOfWork.RoomRepository.Value.CountAsync(roomsSpecification, cancellationToken);
    if (roomsCount >= Constants.Limits.MaxRoomsPerUser) throw new MaxNumberRoomsReachedException(user.Id);

    var integrationEvent = new RoomCreatedIntegrationEvent
    {
      Id = room.Id,
      FilmId = room.FilmId,
      IsSerial = film.IsSerial,
      Owner = new Viewer
      {
        Id = user.Id,
        PhotoKey = user.PhotoKey,
        UserName = user.Username,
        Settings = user.RoomSettings
      }
    };

    await publishEndpoint.Publish(integrationEvent, cancellationToken: cancellationToken);
  }
}