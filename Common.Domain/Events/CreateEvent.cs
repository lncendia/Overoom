namespace Common.Domain.Events;

/// <summary>
/// Событие создания агрегата
/// </summary>
/// <typeparam name="T">Тип агрегата</typeparam>
public class CreateEvent<T> : DomainEvent
{
    /// <summary>
    /// Агрегат
    /// </summary>
    public required T Aggregate { get; init; }
}