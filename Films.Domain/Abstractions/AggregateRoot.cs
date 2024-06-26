namespace Films.Domain.Abstractions;

public abstract class AggregateRoot
{
    public virtual Guid Id { get; } = Guid.NewGuid();
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}