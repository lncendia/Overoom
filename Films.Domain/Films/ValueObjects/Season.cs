namespace Films.Domain.Films.ValueObjects;

/// <summary>
/// Представляет сезон сериала, содержащий коллекцию эпизодов.
/// Реализует сравнение и равенство по номеру сезона.
/// </summary>
public class Season : IEquatable<Season>, IComparable<Season>
{
    /// <summary>
    /// Номер сезона. Используется для идентификации и сортировки.
    /// </summary>
    public required int Number { get; init; }

    /// <summary>
    /// Коллекция эпизодов в сезоне, доступная только для чтения.
    /// </summary>
    public required IReadOnlySet<Episode> Episodes { get; init; }

    /// <summary>
    /// Определяет равенство текущего сезона с другим сезоном по номеру.
    /// </summary>
    /// <param name="other">Сезон для сравнения.</param>
    /// <returns>true если номера сезонов равны; иначе false.</returns>
    public bool Equals(Season? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Number == other.Number;
    }

    /// <summary>
    /// Определяет равенство текущего объекта с другим объектом.
    /// </summary>
    /// <param name="obj">Объект для сравнения.</param>
    /// <returns>true если объекты равны; иначе false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Season)obj);
    }

    /// <summary>
    /// Возвращает хэш-код, основанный на номере сезона.
    /// </summary>
    /// <returns>Хэш-код объекта.</returns>
    public override int GetHashCode() => Number;

    /// <summary>
    /// Сравнивает текущий сезон с другим сезоном по номеру.
    /// </summary>
    /// <param name="other">Сезон для сравнения.</param>
    /// <returns>
    /// 0 если номера равны,
    /// -1 если текущий номер меньше,
    /// 1 если текущий номер больше или other равен null.
    /// </returns>
    public int CompareTo(Season? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Number.CompareTo(other.Number);
    }
}