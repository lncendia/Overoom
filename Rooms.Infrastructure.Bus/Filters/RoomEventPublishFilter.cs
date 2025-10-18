using Common.Application.ScopedDictionary;
using MassTransit;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Events;
using Rooms.Infrastructure.Bus.Services;

namespace Rooms.Infrastructure.Bus.Filters;

/// <summary>
/// Фильтр публикации событий комнат.
/// Он сразу отправляет событие по сокету локальным клиентам
/// и добавляет в заголовки InstanceName, чтобы можно было
/// на стороне consumer'ов отличить инициатора.
/// </summary>
/// <param name="instanceName">Имя текущего инстанса (для заголовка)</param>
/// <param name="sender">Сервис, который умеет рассылать события клиентам по сокету</param>
public class RoomEventPublishFilter(IInstanceName instanceName, IRoomEventSender sender, IScopedContext scopedContext)
    : IFilter<PublishContext<RoomBaseEvent>>
{
    /// <summary>
    /// Логика фильтра: выполняется при публикации события.
    /// </summary>
    public async Task Send(PublishContext<RoomBaseEvent> context, IPipe<PublishContext<RoomBaseEvent>> next)
    {
        // В заголовки события добавляем InstanceName,
        // чтобы на стороне consumer можно было понять, кто его опубликовал.
        context.Headers.Set(Constants.Headers.InstanceName, instanceName.Name);
        
        // Пробуем достать roomId из заголовков.
        // Если его нет — значит событие не привязано к конкретной комнате,
        // и мы не выполняем локальную рассылку.
        var roomId = scopedContext.Current.Get<Guid>(Constants.Headers.RoomId);
        context.Headers.Set(Constants.Headers.RoomId, roomId);

        // Достаём connectionId инициатора (опционально).
        // Может использоваться, чтобы не отсылать событие обратно инициатору.
        if (scopedContext.Current.TryGetValue(Constants.Headers.ExcludedConnectionId, out string? connectionId))
        {
            context.Headers.Set(Constants.Headers.ExcludedConnectionId, connectionId);
        }

        // Передаём событие дальше по конвейеру.
        await next.Send(context);
        
        // Сразу отправляем событие локальным клиентам этой комнаты через сокеты,
        // не дожидаясь, пока сообщение пройдёт через брокер и вернётся.
        await sender.SendAsync(context.Message, roomId, connectionId);
    }

    /// <summary>
    /// Позволяет MassTransit "увидеть" фильтр в пайплайне при диагностике.
    /// </summary>
    public void Probe(ProbeContext context)
    {
        // Регистрируем фильтр с именем "RoomEventPublishFilter"
        context.CreateFilterScope("RoomEventPublishFilter");
    }
}