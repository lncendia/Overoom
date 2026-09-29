namespace Rooms.Domain.Repositories;

/// <summary>
/// Интерфейс Unit of Work для работы с моделями данных кинотеки.
/// Предоставляет ленивую инициализацию репозиториев и управление транзакциями.
/// </summary>
public interface IUnitOfWork
{
  /// <summary>
  /// Лениво инициализируемый репозиторий для работы с сообщениями
  /// </summary>
  Lazy<IMessageRepository> MessageRepository { get; }

  /// <summary>
  /// Лениво инициализируемый репозиторий для работы с комнатами просмотра
  /// </summary>
  Lazy<IRoomRepository> RoomRepository { get; }

  /// <summary>
  /// Асинхронно сохраняет все изменения, внесенные в репозитории, в рамках текущей транзакции, если она открыта
  /// </summary>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Задача, представляющая асинхронную операцию сохранения</returns>
  Task SaveChangesAsync(CancellationToken cancellationToken = default);
}