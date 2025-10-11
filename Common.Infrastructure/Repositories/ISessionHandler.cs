using MongoDB.Driver;

namespace Common.Infrastructure.Repositories;

/// <summary>
/// Интерфейс обработчика сессий MongoDB.
/// Определяет контракт для выполнения операций в рамках сессии.
/// </summary>
public interface ISessionHandler
{
    /// <summary>
    /// Выполняет указанное действие в рамках сессии MongoDB.
    /// </summary>
    /// <param name="action">Действие для выполнения, принимающее сессию и токен отмены.</param>
    /// <param name="token">Токен отмены для асинхронной операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task ExecuteAsync(Func<IClientSessionHandle, CancellationToken, Task> action, CancellationToken token = default);
}