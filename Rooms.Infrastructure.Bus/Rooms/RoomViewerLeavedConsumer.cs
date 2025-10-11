using Common.IntegrationEvents.Rooms;
using MassTransit;
using MediatR;
using Rooms.Application.Abstractions.Commands;

namespace Rooms.Infrastructure.Bus.Rooms;

/// <summary>
/// Обработчик интеграционного события RoomViewerLeavedIntegrationEvent
/// </summary>
/// <param name="mediator">Медиатор</param>
public class RoomViewerLeavedConsumer(ISender mediator) : IConsumer<RoomViewerLeavedIntegrationEvent>
{
    /// <summary>
    /// Метод обработчик 
    /// </summary>
    /// <param name="context">Контекст сообщения</param>
    public Task Consume(ConsumeContext<RoomViewerLeavedIntegrationEvent> context)
    {
        // Получаем данные события
        var integrationEvent = context.Message;

        // Отправляем команду на обработку события
        return mediator.Send(new LeaveCommand
        {
            RoomId = integrationEvent.RoomId,
            ViewerId = integrationEvent.ViewerId
        }, context.CancellationToken);
    }
}