using Common.Application.Events;
using Common.Domain.Events;
using Hangfire;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события удаления комнаты, выполняющий очистку связанных сообщений
/// </summary>
/// <param name="backgroundJobClient">Клиент для постановки фоновых задач.</param>
public class DeleteMessagesEventHandler(IBackgroundJobClientV2 backgroundJobClient)
    : BeforeSaveNotificationHandler<DeleteEvent<Room>>
{
    /// <summary>
    /// Обрабатывает событие удаления комнаты
    /// </summary>
    /// <param name="event">Событие удаления комнаты</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override Task Execute(DeleteEvent<Room> @event, CancellationToken cancellationToken)
    {
        // Помещаем задачу очистки сообщений в фоновую очередь Hangfire
        // Это предотвращает блокировку основного потока при удалении большого количества сообщений
        backgroundJobClient.Enqueue<IMessagesCleaner>(Constants.Hangfire.Queue,
            cleaner => cleaner.CleanAsync(@event.Id, CancellationToken.None)
        );

        // Возвращаем завершенную задачу, так как сама очистка выполняется асинхронно в фоне
        return Task.CompletedTask;
    }
}