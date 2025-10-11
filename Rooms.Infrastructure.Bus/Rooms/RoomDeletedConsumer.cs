using Common.IntegrationEvents.Rooms;
using MassTransit;
using MediatR;
using Rooms.Application.Abstractions.Commands;

namespace Rooms.Infrastructure.Bus.Rooms;

/// <summary>
/// Обработчик интеграционного события RoomDeletedIntegrationEvent
/// </summary>
/// <param name="mediator">Медиатор</param>
public class RoomDeletedConsumer(ISender mediator) : IConsumer<RoomDeletedIntegrationEvent>
{
    /// <summary>
    /// Метод обработчик 
    /// </summary>
    /// <param name="context">Контекст сообщения</param>
    public async Task Consume(ConsumeContext<RoomDeletedIntegrationEvent> context)
    {
        // Получаем данные события
        var integrationEvent = context.Message;

        // Отправляем команду на обработку события
        await mediator.Send(new DeleteRoomCommand
        {
            RoomId = integrationEvent.Id
        }, context.CancellationToken);
    }
}