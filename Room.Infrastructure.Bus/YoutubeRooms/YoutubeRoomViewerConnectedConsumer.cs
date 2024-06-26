using MassTransit;
using MediatR;
using Overoom.IntegrationEvents.Rooms.YoutubeRooms;
using Room.Application.Abstractions.Commands.Rooms;
using Room.Application.Abstractions.Commands.YoutubeRooms;
using Room.Domain.Rooms.Rooms.ValueObjects;

namespace Room.Infrastructure.Bus.YoutubeRooms;

/// <summary>
/// Обработчик интеграционного события YoutubeRoomViewerConnectedIntegrationEvent
/// </summary>
/// <param name="mediator">Медиатор</param>
public class YoutubeRoomViewerConnectedConsumer(ISender mediator)
    : IConsumer<YoutubeRoomViewerConnectedIntegrationEvent>
{
    /// <summary>
    /// Метод обработчик 
    /// </summary>
    /// <param name="context">Контекст сообщения</param>
    public Task Consume(ConsumeContext<YoutubeRoomViewerConnectedIntegrationEvent> context)
    {
        // Получаем данные события
        var integrationEvent = context.Message;

        // Отправляем команду на обработку события
        return mediator.Send(new ConnectCommand
        {
            RoomId = integrationEvent.RoomId,
            Viewer = new ViewerData
            {
                Id = integrationEvent.Viewer.Id,
                Nickname = integrationEvent.Viewer.Name,
                PhotoUrl = integrationEvent.Viewer.PhotoUrl,
                Allows = new Allows
                {
                    Beep = integrationEvent.Viewer.Beep,
                    Scream = integrationEvent.Viewer.Scream,
                    Change = integrationEvent.Viewer.Change
                }
            }
        }, context.CancellationToken);
    }
}