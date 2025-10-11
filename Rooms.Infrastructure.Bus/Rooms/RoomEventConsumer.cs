using MassTransit;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Events;
using Rooms.Infrastructure.Bus.Services;

namespace Rooms.Infrastructure.Bus.Rooms;

/// <summary>
/// Обработчик интеграционного события RoomCreatedIntegrationEvent
/// </summary>
[ExcludeFromConfigureEndpoints]
public class RoomEventConsumer(IInstanceName currentInstanceName, IRoomEventSender sender) : IConsumer<RoomBaseEvent>
{
    /// <summary>
    /// Метод обработчик 
    /// </summary>
    /// <param name="context">Контекст сообщения</param>
    public Task Consume(ConsumeContext<RoomBaseEvent> context)
    {
        var instanceName = context.Headers.Get<string>(Constants.InstanceNameHeader);
        if (instanceName == null || currentInstanceName.Name == instanceName) return Task.CompletedTask;
        
        var roomId = context.Headers.Get<Guid>(Constants.RoomIdHeader);
        if (roomId == null) return Task.CompletedTask;
        
        var connectionId = context.Headers.Get<string>(Constants.ExcludedConnectionIdHeader);
        
        return sender.SendAsync(context.Message, roomId.Value, connectionId);
    }
}