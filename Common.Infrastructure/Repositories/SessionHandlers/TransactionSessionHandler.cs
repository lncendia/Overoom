using MongoDB.Driver;

namespace Common.Infrastructure.Repositories.SessionHandlers;

/// <summary>
/// Обработчик сессии для транзакционных операций.
/// Создает новую транзакцию для каждого вызова
/// </summary>
/// <param name="client">Клиент MongoDB</param>
public class TransactionSessionHandler(IMongoClient client) : ISessionHandler
{
    /// <summary>
    /// Выполняет операцию в рамках транзакции
    /// </summary>
    /// <param name="action">Действие для выполнения</param>
    /// <param name="token">Токен отмены</param>
    public async Task ExecuteAsync(Func<IClientSessionHandle, CancellationToken, Task> action, 
        CancellationToken token = default)
    {
        // Создаем новую сессию MongoDB
        using var sessionHandle = await client.StartSessionAsync(cancellationToken: token);

        // Выполняем действие в рамках транзакции
        await sessionHandle.WithTransactionAsync(async (handle, ct) =>
        {
            await action(handle, ct);
            return true;
        }, cancellationToken: token);
    }
}