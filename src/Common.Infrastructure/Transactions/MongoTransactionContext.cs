using Common.Application.Transactions;

using MassTransit.MongoDbIntegration;

using MongoDB.Driver;

namespace Common.Infrastructure.Transactions;

/// <summary>
/// Контекст транзакции поверх сессии MassTransit.
/// </summary>
/// <remarks>
/// MassTransit хранит сессию в scoped <see cref="MongoDbContext"/>: её же использует bus outbox при публикации
/// и inbox консьюмеров (UseMongoDbOutbox). Поэтому транзакция, открытая здесь, автоматически включает outbox,
/// а внутри консьюмера с inbox репозитории работают в транзакции, открытой MassTransit.
/// Если точка входа транзакцию не открыла, bus outbox при первой публикации открывает её сам и не фиксирует:
/// её фиксирует единица работы (<see cref="CommitImplicitAsync"/>), иначе и сообщение, и изменения пропадут.
/// </remarks>
/// <param name="dbContext">Контекст MongoDB MassTransit</param>
public class MongoTransactionContext(MongoDbContext dbContext) : ITransactionContext, ITransactionManager
{
  private readonly List<Func<CancellationToken, Task>> _onCommitted = [];

  /// <summary>
  /// Транзакция открыта этим контекстом (а не, например, inbox MassTransit)
  /// </summary>
  private bool _owned;

  /// <inheritdoc/>
  public IClientSessionHandle? Session => dbContext.Session is { IsInTransaction: true } session ? session : null;

  /// <inheritdoc/>
  public async Task BeginAsync(CancellationToken token = default)
  {
    if (Session != null) throw new InvalidOperationException("Transaction is already started.");

    await dbContext.BeginTransaction(token);
    _owned = true;
  }

  /// <inheritdoc/>
  public async Task CommitAsync(CancellationToken token = default)
  {
    EnsureOwned();
    await dbContext.CommitTransaction(token);
    _owned = false;

    Func<CancellationToken, Task>[] actions = [.. _onCommitted];
    _onCommitted.Clear();

    foreach (Func<CancellationToken, Task> action in actions)
    {
      await action(token);
    }
  }

  /// <inheritdoc/>
  public async Task AbortAsync(CancellationToken token = default)
  {
    EnsureOwned();
    _onCommitted.Clear();
    _owned = false;
    await dbContext.AbortTransaction(token);
  }

  /// <inheritdoc/>
  public Task CommitImplicitAsync(CancellationToken token = default)
  {
    if (_owned) throw new InvalidOperationException("Transaction was started by this context and is committed by it.");
    return dbContext.CommitTransaction(token);
  }

  /// <inheritdoc/>
  public Task AbortImplicitAsync(CancellationToken token = default)
  {
    if (_owned) throw new InvalidOperationException("Transaction was started by this context and is aborted by it.");
    return dbContext.AbortTransaction(token);
  }

  /// <inheritdoc/>
  public Task OnCommittedAsync(Func<CancellationToken, Task> action, CancellationToken token = default)
  {
    // Момент фиксации чужой транзакции (inbox MassTransit) нам неизвестен, поэтому выполняем сразу
    if (!_owned) return action(token);

    _onCommitted.Add(action);
    return Task.CompletedTask;
  }

  /// <summary>
  /// Проверяет, что транзакция открыта этим контекстом
  /// </summary>
  private void EnsureOwned()
  {
    if (!_owned) throw new InvalidOperationException("Transaction was not started by this context.");
  }
}
