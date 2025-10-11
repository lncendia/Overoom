namespace Films.Domain.Films.ValueObjects;

/// <summary>
/// Представляет эпизод сериала, содержащий информацию о версиях медиаконтента.
/// Наследует от <see cref="MediaContent"/> и реализует сравнение по номеру эпизода.
/// </summary>
public class Episode : MediaContent, IEquatable<Episode>, IComparable<Episode>
{
    /// <summary>
    /// Номер эпизода в сезоне. Используется для идентификации и сортировки.
    /// </summary>
    public int Number { get; init; }

    /// <summary>
    /// Определяет равенство текущего эпизода с другим эпизодом по номеру.
    /// </summary>
    /// <param name="other">Эпизод для сравнения.</param>
    /// <returns>true если номера эпизодов равны; иначе false.</returns>
    public bool Equals(Episode? other)
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
        return Equals((Episode)obj);
    }

    /// <summary>
    /// Возвращает хэш-код, основанный на номере эпизода.
    /// </summary>
    /// <returns>Хэш-код объекта.</returns>
    public override int GetHashCode() => Number;

    /// <summary>
    /// Сравнивает текущий эпизод с другим эпизодом по номеру.
    /// </summary>
    /// <param name="other">Эпизод для сравнения.</param>
    /// <returns>
    /// 0 если номера равны,
    /// -1 если текущий номер меньше,
    /// 1 если текущий номер больше или other равен null.
    /// </returns>
    public int CompareTo(Episode? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Number.CompareTo(other.Number);
    }
}