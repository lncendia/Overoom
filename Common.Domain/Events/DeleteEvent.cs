namespace Common.Domain.Events;

/// <summary>
/// Событие удаления агрегата
/// </summary>
/// <typeparam name="T">Тип агрегата</typeparam>
public class DeleteEvent<T> : DomainEvent
{
    /// <summary>
    /// Идентификатор удаленного агрегата
    /// </summary>
    public required Guid Id { get; init; }
}