using MassTransit;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Events;
using Rooms.Infrastructure.Bus.Services;

namespace Rooms.Infrastructure.Bus.Rooms;

/// <summary>
/// Обработчик интеграционного события RoomCreatedIntegrationEvent
/// </summary>
[ExcludeFromConfigureEndpoints]
public class RoomEventConsumer(IInstanceName currentInstanceName, IRoomEventSender sender) 
    : IConsumer<RoomBaseEvent>
{
    /// <summary>
    /// Метод-обработчик события, вызываемый при получении сообщения из шины.
    /// </summary>
    /// <param name="context">Контекст сообщения, содержащий данные и метаданные события.</param>
    public Task Consume(ConsumeContext<RoomBaseEvent> context)
    {
        // Извлекаем имя инстанса, отправившего сообщение
        var instanceName = context.Headers.Get<string>(Constants.Headers.InstanceName);

        // Если имя не указано или сообщение пришло от текущего инстанса — игнорируем (чтобы не было самоповтора)
        if (instanceName == null || currentInstanceName.Name == instanceName) 
            return Task.CompletedTask;
        
        // Извлекаем идентификатор комнаты, к которой относится событие
        var roomId = context.Headers.Get<Guid>(Constants.Headers.RoomId);

        // Если идентификатор отсутствует — сообщение некорректное, пропускаем
        if (roomId == null) 
            return Task.CompletedTask;
        
        // Извлекаем идентификатор соединения, которому не нужно пересылать событие (например, инициатору)
        var connectionId = context.Headers.Get<string>(Constants.Headers.ExcludedConnectionId);
        
        // Отправляем событие во внутренний сервис рассылки событий комнатам
        return sender.SendAsync(context.Message, roomId.Value, connectionId);
    }
}
