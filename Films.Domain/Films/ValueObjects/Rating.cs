namespace Films.Domain.Films.ValueObjects;

using System;

/// <summary>
/// Значение рейтинга фильма на КиноПоиске (от 0.0 до 10.0 включительно).
/// </summary>
public readonly record struct Rating
{
    /// <summary>
    /// Числовое значение рейтинга.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Создаёт новый экземпляр <see cref="Rating"/>.
    /// </summary>
    /// <param name="value">Значение рейтинга от 0.0 до 10.0 включительно.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Возникает, если <paramref name="value"/> меньше 0.0 или больше 10.0.
    /// </exception>
    public Rating(double value)
    {
        if (value is < 0 or > 10)
            throw new ArgumentOutOfRangeException(nameof(value), "Рейтинг должен быть от 0 до 10.");

        Value = value;
    }
}
