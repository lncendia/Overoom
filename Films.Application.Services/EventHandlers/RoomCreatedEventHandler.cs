using Common.Application.Events;
using Common.IntegrationEvents.Rooms;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using Films.Domain.Rooms.Events;
using Films.Domain.Rooms.Specifications;
using MassTransit;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Обработчик доменного события создания комнаты
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
public class RoomCreatedEventHandler(IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint) : BeforeSaveNotificationHandler<RoomCreatedEvent>
{
    /// <summary>
    /// Обрабатывает событие создания комнаты и публикует интеграционное событие
    /// </summary>
    /// <param name="notification">Доменное событие создания комнаты</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(RoomCreatedEvent notification, CancellationToken cancellationToken)
    {
        var room = notification.Room;
        var user = notification.Owner;
        var film = notification.Film;
        
        // Создаем спецификацию для поиска комнат, созданных текущим пользователем
        var roomsSpecification = new RoomByUserSpecification(user.Id);
        
        // Получаем количество комнат, созданных пользователем
        var roomsCount = await unitOfWork.RoomRepository.Value.CountAsync(roomsSpecification, cancellationToken);

        // Проверяем, не превысил ли пользователь лимит созданных комнат (5 комнаты)
        if (roomsCount >= Constants.MaxRoomsCount) throw new MaxNumberRoomsReachedException(user.Id);
        
        // Создаем событие интеграции с полной информацией о созданной комнате
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
        
        // Публикуем событие интеграции через MassTransit
        await publishEndpoint.Publish(integrationEvent, cancellationToken: cancellationToken);
    }
}