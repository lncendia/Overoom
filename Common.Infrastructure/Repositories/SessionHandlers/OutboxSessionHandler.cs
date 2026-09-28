using MassTransit.MongoDbIntegration;
using MongoDB.Driver;

namespace Common.Infrastructure.Repositories.SessionHandlers;

/// <summary>
/// Обработчик сессии для работы с outbox паттерном MassTransit.
/// Обеспечивает атомарность сохранения данных и отправки сообщений.
/// </summary>
/// <param name="dbContext">Контекст базы данных MongoDB.</param>
public class OutboxSessionHandler(MongoDbContext dbContext) : ISessionHandler
{
  /// <summary>
  /// Выполняет действие перед сохранением в рамках транзакции Outbox.
  /// </summary>
  /// <param name="action">Действие для выполнения</param>
  /// <param name="token">Токен отмены</param>
  public async Task BeforeSaveExecuteAsync(Func<CancellationToken, Task> action, CancellationToken token = default)
  {
    await dbContext.BeginTransaction(token);
    await action(token);
  }

  /// <summary>
  /// Выполняет операцию в рамках транзакции outbox
  /// </summary>
  /// <param name="action">Действие для выполнения</param>
  /// <param name="token">Токен отмены</param>
  /// <exception cref="InvalidOperationException">Если сессия не инициализирована</exception>
  public async Task ExecuteAsync(Func<IClientSessionHandle, CancellationToken, Task> action,
    CancellationToken token = default)
  {
    await action(dbContext.Session!, token);
    await dbContext.CommitTransaction(token);
  }
}