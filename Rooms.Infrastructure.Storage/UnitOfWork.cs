using Common.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Rooms.Domain.Repositories;
using Rooms.Infrastructure.Storage.Context;
using Rooms.Infrastructure.Storage.Repositories;

namespace Rooms.Infrastructure.Storage;

/// <summary>
/// Класс, реализующий интерфейс IUnitOfWork.
/// Представляет собой единицу работы, которая отслеживает все изменения, внесенные в репозитории,
/// и предоставляет метод для сохранения этих изменений в базе данных.
/// </summary>
public class UnitOfWork : UnitOfWorkBase, IUnitOfWork
{
    /// <summary>
    /// Инициализирует новый экземпляр класса UnitOfWork.
    /// </summary>
    /// <param name="handlerFactory">Фабрика для создания обработчиков сессий MongoDB.</param>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="publisher">Публикатор событий.</param>
    /// <param name="logger">Логгер.</param>
    public UnitOfWork(ISessionHandlerFactory handlerFactory, MongoDbContext context, IPublisher publisher, ILogger<UnitOfWork> logger) 
        : base(handlerFactory, publisher, logger)
    {
        // Создаем RoomRepository
        RoomRepository = new Lazy<IRoomRepository>(() => new RoomRepository(context));

        // Создаем MessageRepository
        MessageRepository = new Lazy<IMessageRepository>(() => new MessageRepository(context));
    }

    /// <summary>
    /// Лениво инициализируемый репозиторий для работы с комнатами просмотра
    /// </summary>
    public Lazy<IRoomRepository> RoomRepository { get; }

    /// <summary>
    /// Лениво инициализируемый репозиторий для работы с сообщениями
    /// </summary>
    public Lazy<IMessageRepository> MessageRepository { get; }

    /// <inheritdoc/>
    /// <summary>
    /// Получает коллекцию репозиториев, в которых были изменения
    /// </summary>
    protected override IEnumerable<IRepository> GetRepositories()
    {
        // Проверяем, были ли созданы изменения в репозитории RoomRepository.
        if (RoomRepository.IsValueCreated)
            yield return (RoomRepository)RoomRepository.Value;
    
        // Проверяем, были ли созданы изменения в репозитории MessageRepository.
        if (MessageRepository.IsValueCreated)
            yield return (MessageRepository)MessageRepository.Value;
    }
}