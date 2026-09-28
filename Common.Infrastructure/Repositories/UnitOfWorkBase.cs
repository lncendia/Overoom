using System.Diagnostics;

using Common.Domain.Events;
using Common.Infrastructure.Repositories.Metrics;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Common.Infrastructure.Repositories;

/// <summary>
/// Базовый абстрактный класс для реализации Unit of Work паттерна.
/// Координирует работу репозиториев и обеспечивает атомарность операций.
/// </summary>
/// <param name="handlerFactory">Фабрика для создания обработчиков сессий MongoDB.</param>
/// <param name="publisher">Сервис публикации доменных событий (MediatR).</param>
/// <param name="logger">Логгер для записи информации о выполнении операций.</param>
public abstract class UnitOfWorkBase(
  ISessionHandlerFactory handlerFactory,
  IPublisher publisher,
  ILogger<UnitOfWorkBase> logger)
{
  /// <summary>
  /// Асинхронно сохраняет все изменения, внесенные в репозитории, и завершает транзакцию.
  /// Также отправляет все события доменной модели, которые были зарегистрированы в контексте.
  /// </summary>
  public async Task SaveChangesAsync(ISessionHandler? handler = null, CancellationToken token = default)
  {
    handler ??= handlerFactory.CreateDefaultHandler();
    await handler.BeforeSaveExecuteAsync(BeforeCommitSessionAsync, token);
    var stopwatch = Stopwatch.StartNew();
    await handler.ExecuteAsync(ApplyChanges, token);
    stopwatch.Stop();
    logger.LogInformation("Transaction commited in {elapsed} ms.", stopwatch.ElapsedMilliseconds);
    RepositoryMetrics.TransactionDuration.Record(stopwatch.Elapsed.TotalMilliseconds);
    RepositoryMetrics.TransactionsCommitted.Add(1);
    await AfterCommitSessionAsync(token);
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