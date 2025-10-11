using MassTransit.MongoDbIntegration;
using MongoDB.Driver;

namespace Common.Infrastructure.Repositories.SessionHandlers;

/// <summary>
/// Обработчик сессии для работы с inbox паттерном MassTransit
/// Использует существующую сессию из MongoDbContext для обеспечения согласованности
/// </summary>
/// <param name="dbContext">Контекст базы данных MongoDB</param>
public class InboxSessionHandler(MongoDbContext dbContext) : ISessionHandler
{
    /// <summary>
    /// Выполняет операцию в рамках существующей сессии inbox
    /// </summary>
    /// <param name="action">Действие для выполнения</param>
    /// <param name="token">Токен отмены</param>
    /// <exception cref="InvalidOperationException">Если сессия не инициализирована</exception>
    public async Task ExecuteAsync(Func<IClientSessionHandle, CancellationToken, Task> action,
        CancellationToken token = default)
    {
        // Проверяем наличие активной сессии
        if (dbContext.Session == null) throw new InvalidOperationException("Session is null");

        // Выполняем действие в контексте существующей сессии
        await action(dbContext.Session, token);
    }
}