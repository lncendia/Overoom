namespace Common.Infrastructure.Repositories;

/// <summary>
/// Фабрика для создания обработчиков сессий MongoDB.
/// Предоставляет методы для создания различных типов обработчиков сессий.
/// </summary>
public interface ISessionHandlerFactory
{
    /// <summary>
    /// Создает обработчик сессии для работы с inbox паттерном MassTransit.
    /// </summary>
    /// <returns>Экземпляр ISessionHandler для inbox операций.</returns>
    ISessionHandler CreateInboxHandler();
    
    /// <summary>
    /// Создает обработчик сессии для работы с outbox паттерном MassTransit.
    /// </summary>
    /// <returns>Экземпляр ISessionHandler для outbox операций.</returns>
    ISessionHandler CreateOutboxHandler();
    
    /// <summary>
    /// Создает обработчик сессии для транзакционных операций.
    /// </summary>
    /// <returns>Экземпляр ISessionHandler для транзакций.</returns>
    ISessionHandler CreateTransactionHandler();
    
    /// <summary>
    /// Создает обработчик сессии по умолчанию для нетранзакционных операций.
    /// </summary>
    /// <returns>Экземпляр ISessionHandler для операций по умолчанию.</returns>
    ISessionHandler CreateDefaultHandler();
}