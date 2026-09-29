using System.Diagnostics;

using Common.Domain.Events;
using Common.Infrastructure.Repositories.Metrics;
using Common.Infrastructure.Transactions;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Common.Infrastructure.Repositories;

/// <summary>
/// Базовый абстрактный класс для реализации Unit of Work паттерна.
/// Координирует работу репозиториев и обеспечивает атомарность операций.
/// </summary>
/// <remarks>
/// Сам транзакцию не открывает: если точка входа открыла её (<see cref="ITransactionContext"/>),
/// события до сохранения и запись изменений выполняются в ней, а события после сохранения откладываются
/// до её фиксации. Без транзакции изменения записываются в отдельной сессии, как независимые операции.
/// Исключение — транзакция, которую bus outbox MassTransit открыл сам, когда обработчик события до сохранения
/// опубликовал сообщение: изменения записываются в неё, и она фиксируется здесь же, чтобы сообщение
/// и изменения сохранились вместе.
/// </remarks>
/// <param name="client">Клиент MongoDB для записи вне транзакции.</param>
/// <param name="transaction">Текущая транзакция области.</param>
/// <param name="publisher">Сервис публикации доменных событий (MediatR).</param>
/// <param name="logger">Логгер для записи информации о выполнении операций.</param>
public abstract class UnitOfWorkBase(
  IMongoClient client,
  ITransactionContext transaction,
  IPublisher publisher,
  ILogger<UnitOfWorkBase> logger)
{
  /// <summary>
  /// Асинхронно сохраняет все изменения, внесенные в репозитории.
  /// Также отправляет все события доменной модели, которые были зарегистрированы в контексте.
  /// </summary>
  public async Task SaveChangesAsync(CancellationToken token = default)
  {
    bool hasEntryPointTransaction = transaction.Session != null;
    var stopwatch = Stopwatch.StartNew();

    try
    {
      await BeforeCommitSessionAsync(token);

      if (transaction.Session != null)
      {
        await ApplyChanges(transaction.Session, token);
        if (!hasEntryPointTransaction) await transaction.CommitImplicitAsync(token);
      }
      else
      {
        using IClientSessionHandle session = await client.StartSessionAsync(cancellationToken: token);
        await ApplyChanges(session, token);
      }
    }
    catch when (!hasEntryPointTransaction && transaction.Session != null)
    {
      await transaction.AbortImplicitAsync(CancellationToken.None);
      throw;
    }

    stopwatch.Stop();
    logger.LogInformation("Changes saved in {elapsed} ms.", stopwatch.ElapsedMilliseconds);
    RepositoryMetrics.TransactionDuration.Record(stopwatch.Elapsed.TotalMilliseconds);
    RepositoryMetrics.TransactionsCommitted.Add(1);
    await transaction.OnCommittedAsync(AfterCommitSessionAsync, token);
  }

  /// <summary>
  /// Применяет все изменения, внесенные в репозитории, к базе данных в рамках указанной сессии MongoDB.
  /// </summary>
  /// <param name="sessionHandle">Существующая сессия MongoDB, в которой будут применены изменения.</param>
  /// <param name="token">Токен отмены для отслеживания отмены операции.</param>
  /// <remarks>
  /// Метод проверяет, были ли созданы изменения в каждом из репозиториев.
  /// Если изменения есть, они применяются к соответствующей коллекции базы данных через метод CommitChangesAsync.
  /// </remarks>
  private async Task ApplyChanges(IClientSessionHandle sessionHandle, CancellationToken token)
  {
    IRepository[] repositories = [.. GetRepositories()];

    foreach (IRepository repository in repositories)
    {
      await repository.CommitAsync(sessionHandle, token);
    }
  }

  /// <summary>
  /// Выполняет обработку событий перед подтверждением транзакции
  /// </summary>
  /// <param name="token">Токен отмены для асинхронной операции</param>
  /// <remarks>
  /// Метод обрабатывает все доменные события, помеченные для выполнения перед сохранением.
  /// </remarks>
  private async Task BeforeCommitSessionAsync(CancellationToken token = default)
  {
    IRepository[] repositories = [.. GetRepositories()];
    IEnumerable<DomainEvent> domainEvents = repositories.SelectMany(r => r.Events);
    var stopwatch = Stopwatch.StartNew();

    foreach (DomainEvent domainEvent in domainEvents)
    {
      domainEvent.BeforeSave = true;
      await publisher.Publish(domainEvent, token);
    }

    stopwatch.Stop();
    logger.LogInformation("Before save events executed in {elapsed} ms.", stopwatch.ElapsedMilliseconds);
    RepositoryMetrics.BeforeCommitDuration.Record(stopwatch.Elapsed.TotalMilliseconds);
  }

  /// <summary>
  /// Выполняет обработку событий после подтверждения транзакции
  /// </summary>
  /// <param name="token">Токен отмены для асинхронной операции</param>
  /// <remarks>
  /// Метод обрабатывает все доменные события, помеченные для выполнения после сохранения.
  /// В случае ошибки при обработке события, ошибка логируется, но не прерывает выполнение.
  /// </remarks>
  private async Task AfterCommitSessionAsync(CancellationToken token = default)
  {
    IRepository[] repositories = [.. GetRepositories()];
    IEnumerable<DomainEvent> domainEvents = repositories.SelectMany(r => r.Events);
    var stopwatch = Stopwatch.StartNew();

    foreach (DomainEvent domainEvent in domainEvents)
    {
      try
      {
        domainEvent.BeforeSave = false;
        await publisher.Publish(domainEvent, token);
      }
      catch (Exception ex)
      {
        logger.LogWarning(ex, "An error occured while executing after save event.");
      }
    }

    stopwatch.Stop();
    logger.LogInformation("After save events executed in {elapsed} ms.", stopwatch.ElapsedMilliseconds);
    RepositoryMetrics.AfterCommitDuration.Record(stopwatch.Elapsed.TotalMilliseconds);
  }

  /// <summary>
  /// Получает коллекцию репозиториев, в которых были изменения
  /// </summary>
  /// <returns>Коллекция измененных репозиториев</returns>
  protected abstract IEnumerable<IRepository> GetRepositories();
}
