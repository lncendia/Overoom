using MassTransit;
using Rooms.Application.Abstractions.Services;

namespace Rooms.Infrastructure.Bus.Rooms;

/// <summary>
/// Обработчик события CleanMessages
/// </summary>
/// <param name="cleaner">Сервис очистки сообщений</param>
public class CleanMessagesConsumer(IMessagesCleaner cleaner) : IConsumer<CleanMessages>
{
    /// <summary>
    /// Метод обработчик 
    /// </summary>
    /// <param name="context">Контекст сообщения</param>
    public async Task Consume(ConsumeContext<CleanMessages> context)
    {
        // Запускаем отчистку сообщений
        await cleaner.CleanAsync(context.Message.RoomId, context.CancellationToken);
    }
}