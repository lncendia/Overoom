using Common.Domain.Events;
using MongoDB.Driver;

namespace Common.Infrastructure.Repositories;

/// <summary>
/// Интерфейс репозитория, определяющий базовый контракт для работы с хранилищем данных
/// </summary>
/// <remarks>
/// Предоставляет методы для управления доменными событиями и сохранения изменений
/// </remarks>
public interface IRepository
{
    /// <summary>
    /// Коллекция доменных событий, связанных с изменениями в репозитории
    /// </summary>
    /// <value>
    /// Доступная только для чтения коллекция событий, которые должны быть обработаны
    /// </value>
    IReadOnlySet<DomainEvent> Events { get; }
    
    /// <summary>
    /// Асинхронно сохраняет все изменения в хранилище данных в рамках указанной сессии
    /// </summary>
    /// <param name="sessionHandle">Сессия работы с базой данных</param>
    /// <param name="token">Токен отмены для асинхронной операции</param>
    /// <returns>Задача, представляющая асинхронную операцию сохранения</returns>
    /// <remarks>
    /// Реализации должны гарантировать атомарное сохранение всех изменений
    /// в рамках предоставленной сессии
    /// </remarks>
    Task CommitAsync(IClientSessionHandle sessionHandle, CancellationToken token);
}