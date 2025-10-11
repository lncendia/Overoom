using System.Diagnostics.CodeAnalysis;

namespace Common.Domain.Events;

/// <summary>
/// Событие сохранения агрегата
/// </summary>
/// <typeparam name="T">Тип агрегата</typeparam>
public class SaveEvent<T> : DomainEvent, IEquatable<SaveEvent<T>>
{
    /// <summary>
    /// Сохраненный агрегат
    /// </summary>
    [NotNull]
    public required T Aggregate { get; init; }

    public bool Equals(SaveEvent<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<T>.Default.Equals(Aggregate, other.Aggregate);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((SaveEvent<T>)obj);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(Aggregate);
    }
}