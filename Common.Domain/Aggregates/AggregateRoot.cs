using Common.Domain.Events;

namespace Common.Domain.Aggregates;

/// <summary>
/// Базовый класс для агрегатов доменной модели
/// </summary>
public abstract class AggregateRoot
{
    private readonly HashSet<DomainEvent> _domainEvents = [];
    
    /// <summary>
    /// Конструктор базового агрегата
    /// </summary>
    /// <param name="guid">Уникальный идентификатор агрегата</param>
    protected AggregateRoot(Guid guid)
    {
        Id = guid;
    }
    
    /// <summary>
    /// Уникальный идентификатор агрегата
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Коллекция доменных событий агрегата
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents;

    /// <summary>
    /// Добавляет доменное событие в коллекцию
    /// </summary>
    /// <param name="domainEvent">Доменное событие</param>
    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}