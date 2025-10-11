using Common.IntegrationEvents.Rooms;
using MassTransit;
using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.DTOs;

namespace Rooms.Infrastructure.Bus.Rooms;

/// <summary>
/// Обработчик интеграционного события RoomViewerConnectedIntegrationEvent
/// </summary>
/// <param name="mediator">Медиатор</param>
public class RoomViewerJoinedConsumer(ISender mediator) : IConsumer<RoomViewerJoinedIntegrationEvent>
{
    /// <summary>
    /// Метод обработчик 
    /// </summary>
    /// <param name="context">Контекст сообщения</param>
    public async Task Consume(ConsumeContext<RoomViewerJoinedIntegrationEvent> context)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        
        // Получаем данные события
        var integrationEvent = context.Message;

        // Отправляем команду на обработку события
        await mediator.Send(new JoinCommand
        {
            RoomId = integrationEvent.RoomId,
            Viewer = new ViewerData
            {
                Id = integrationEvent.Viewer.Id,
                UserName = integrationEvent.Viewer.UserName,
                PhotoKey = integrationEvent.Viewer.PhotoKey,
                Settings = integrationEvent.Viewer.Settings
            }
        }, context.CancellationToken);
    }
}