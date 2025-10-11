using MassTransit.MongoDbIntegration;
using MongoDB.Driver;

namespace Common.Infrastructure.Repositories.SessionHandlers;

/// <summary>
/// Реализация фабрики обработчиков сессий MongoDB.
/// Создает конкретные экземпляры обработчиков сессий для различных сценариев.
/// </summary>
/// <param name="client">Клиент MongoDB для подключения к базе данных.</param>
/// <param name="dbContext">Контекст базы данных MongoDB с информацией о сессии.</param>
public class SessionHandlerFactory(IMongoClient client, MongoDbContext dbContext) : ISessionHandlerFactory
{
    /// <summary>
    /// Создает обработчик сессии для inbox паттерна MassTransit.
    /// Использует существующую сессию из контекста базы данных.
    /// </summary>
    /// <returns>Обработчик сессии для inbox операций.</returns>
    public ISessionHandler CreateInboxHandler() => new InboxSessionHandler(dbContext);
    
    /// <summary>
    /// Создает обработчик сессии для outbox паттерна MassTransit.
    /// Обеспечивает атомарность сохранения данных и сообщений.
    /// </summary>
    /// <returns>Обработчик сессии для outbox операций.</returns>
    public ISessionHandler CreateOutboxHandler() => new OutboxSessionHandler(dbContext);
    
    /// <summary>
    /// Создает обработчик сессии для транзакционных операций.
    /// Создает новую транзакцию для каждого вызова.
    /// </summary>
    /// <returns>Обработчик сессии для транзакций.</returns>
    public ISessionHandler CreateTransactionHandler() => new TransactionSessionHandler(client);
    
    /// <summary>
    /// Создает обработчик сессии по умолчанию.
    /// Используется для нетранзакционных операций.
    /// </summary>
    /// <returns>Обработчик сессии для операций по умолчанию.</returns>
    public ISessionHandler CreateDefaultHandler() => new DefaultSessionHandler(client);
}