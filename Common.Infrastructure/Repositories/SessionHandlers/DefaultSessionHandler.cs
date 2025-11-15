using MongoDB.Driver;

namespace Common.Infrastructure.Repositories.SessionHandlers;

/// <summary>
/// Стандартный обработчик сессии для нетранзакционных операций
/// </summary>
/// <param name="client">Клиент MongoDB</param>
public class DefaultSessionHandler(IMongoClient client) : ISessionHandler
{
    /// <summary>
    /// Выполняет действие перед сохранением данных.
    /// В этой реализации просто напрямую вызывает переданное действие.
    /// </summary>
    /// <param name="action">Асинхронное действие, которое нужно выполнить перед сохранением.</param>
    /// <param name="token">Токен отмены операции.</param>
    public Task BeforeSaveExecuteAsync(Func<CancellationToken, Task> action, CancellationToken token = default)
    {
        // Выполняем действие
        return action(token);
    }
    
    /// <summary>
    /// Выполняет операцию в рамках сессии без транзакции
    /// </summary>
    /// <param name="action">Действие для выполнения</param>
    /// <param name="token">Токен отмены</param>
    public async Task ExecuteAsync(Func<IClientSessionHandle, CancellationToken, Task> action,
        CancellationToken token = default)
    {
        // Создаем новую сессию MongoDB
        using var sessionHandle = await client.StartSessionAsync(cancellationToken: token);

        // Выполняем действие без транзакции
        await action(sessionHandle, token);
    }
}