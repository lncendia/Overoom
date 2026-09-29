using Common.Infrastructure.Repositories;
using Common.Infrastructure.Transactions;

using Incendia.MongoTracker.Builders;

using MediatR;
using Microsoft.Extensions.Logging;

using MongoDB.Driver;
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
  /// <param name="client">Клиент MongoDB</param>
  /// <param name="transaction">Текущая транзакция области</param>
  /// <param name="context">Контекст базы данных.</param>
  /// <param name="config">Конфигурация трекера</param>
  /// <param name="publisher">Публикатор событий.</param>
  /// <param name="logger">Логгер.</param>
  public UnitOfWork(IMongoClient client, ITransactionContext transaction, MongoDbContext context,
    ModelBuilder config, IPublisher publisher, ILogger<UnitOfWork> logger)
    : base(client, transaction, publisher, logger)
  {
    RoomRepository = new Lazy<IRoomRepository>(() => new RoomRepository(context, config, transaction));
    MessageRepository = new Lazy<IMessageRepository>(() => new MessageRepository(context, config, transaction));
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
    if (RoomRepository.IsValueCreated)
      yield return (RoomRepository)RoomRepository.Value;

    if (MessageRepository.IsValueCreated)
      yield return (MessageRepository)MessageRepository.Value;
  }
}
