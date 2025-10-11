using Common.IntegrationEvents.Rooms;
using MassTransit;
using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.DTOs;

namespace Rooms.Infrastructure.Bus.Rooms;

/// <summary>
/// Обработчик интеграционного события RoomCreatedIntegrationEvent
/// </summary>
/// <param name="mediator">Медиатор</param>
public class RoomCreatedConsumer(ISender mediator) : IConsumer<RoomCreatedIntegrationEvent>
{
    /// <summary>
    /// Метод обработчик 
    /// </summary>
    /// <param name="context">Контекст сообщения</param>
    public async Task Consume(ConsumeContext<RoomCreatedIntegrationEvent> context)
    {
        // Получаем данные события
        var integrationEvent = context.Message;

        // Отправляем команду на обработку события
        await mediator.Send(new CreateRoomCommand
        {
            Id = integrationEvent.Id,
            Owner = new ViewerData
            {
                Id = integrationEvent.Owner.Id,
                UserName = integrationEvent.Owner.UserName,
                PhotoKey = integrationEvent.Owner.PhotoKey,
                Settings = integrationEvent.Owner.Settings
            },
            FilmId = integrationEvent.FilmId,
            IsSerial = integrationEvent.IsSerial
        }, context.CancellationToken);
    }
}