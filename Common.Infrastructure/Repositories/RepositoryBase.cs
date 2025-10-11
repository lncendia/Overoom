using System.Linq.Expressions;
using Common.Domain.Aggregates;
using Common.Domain.Events;
using MongoDB.Driver;
using MongoTracker.Entities;
using MongoTracker.Tracker;

namespace Common.Infrastructure.Repositories;

/// <summary>
/// Базовый класс репозитория для работы с MongoDB
/// </summary>
/// <param name="getIdExpression">Выражение для получения идентификатора сущности</param>
/// <param name="collection">Коллекция MongoDB</param>
/// <typeparam name="T">Тип сущности, наследуемый от UpdatedEntity</typeparam>
/// <typeparam name="TA">Тип агрегата</typeparam>
public abstract class RepositoryBase<T, TA>(
    Expression<Func<T, Guid>> getIdExpression,
    IMongoCollection<T> collection)
    : MongoTracker<Guid, T>(getIdExpression), IRepository
    where T : UpdatedEntity<T>
    where TA : AggregateRoot
{
    /// <summary>
    /// Коллекция доменных событий, связанных с изменениями в репозитории
    /// </summary>
    private readonly HashSet<DomainEvent> _events = [];

    /// <summary>
    /// Доступ только для чтения к коллекции доменных событий
    /// </summary>
    public IReadOnlySet<DomainEvent> Events => _events;

    /// <summary>
    /// Коллекция MongoDB, с которой работает репозиторий
    /// </summary>
    protected IMongoCollection<T> Collection { get; } = collection;

    /// <inheritdoc cref="IRepository"/>
    /// <summary>
    /// Добавляет доменные события в коллекцию репозитория
    /// </summary>
    /// <param name="aggregate">Агрегат для обновления.</param>
    private void AddEvents(TA aggregate)
    {
        foreach (var @event in aggregate.DomainEvents)
        {
            _events.Add(@event);
        }
    }

    /// <inheritdoc cref="IRepository"/>
    /// <summary>
    /// Применяет все изменения в репозитории к базе данных в рамках текущей сессии
    /// </summary>
    /// <param name="sessionHandle">Сессия MongoDB</param>
    /// <param name="token">Токен отмены операции</param>
    public async Task CommitAsync(IClientSessionHandle sessionHandle, CancellationToken token)
    {
        // Получаем список операций для массового выполнения
        var bulkOperations = Commit();

        // Если есть изменения для применения
        if (bulkOperations.Count > 0)
        {
            // Выполняем все операции одним запросом к базе данных
            // BulkWriteAsync выполняет все операции асинхронно в рамках транзакции
            await Collection.BulkWriteAsync(
                sessionHandle,
                bulkOperations,
                cancellationToken: token);
        }
    }

    /// <summary>
    /// Добавляет сущность в репозиторий и регистрирует событие создания агрегата
    /// </summary>
    /// <param name="entity">Сущность для добавления в репозиторий</param>
    /// <param name="aggregate">Агрегат, связанный с добавляемой сущностью</param>
    protected void Add(T entity, TA aggregate)
    {
        // Добавляем сущность в репозиторий для последующего сохранения
        Add(entity);
    
        // Добавляем доменные события из агрегата в коллекцию событий
        AddEvents(aggregate);
    
        // Создаем и добавляем событие создания агрегата
        _events.Add(new CreateEvent<TA> { Aggregate = aggregate });
        
        // Создаем и добавляем событие сохранения агрегата
        _events.Add(new SaveEvent<TA> { Aggregate = aggregate });
    }

    /// <summary>
    /// Регистрирует событие обновления агрегата
    /// </summary>
    /// <param name="aggregate">Обновленный агрегат</param>
    protected void Update(TA aggregate)
    {
        // Добавляем доменные события из агрегата в коллекцию событий
        AddEvents(aggregate);
        
        // Создаем и добавляем событие сохранения агрегата
        _events.Add(new SaveEvent<TA> { Aggregate = aggregate });
    }

    /// <summary>
    /// Удаляет сущность из репозитория и регистрирует событие удаления агрегата
    /// </summary>
    /// <param name="id">Идентификатор сущности для удаления</param>
    protected void Delete(Guid id)
    {
        // Удаляем сущность из репозитория
        Remove(id);
    
        // Создаем и добавляем событие удаления агрегата
        _events.Add(new DeleteEvent<TA> { Id = id });
    }
}