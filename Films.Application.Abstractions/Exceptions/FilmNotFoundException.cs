namespace Films.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда не удается найти фильм.
/// </summary>
public class FilmNotFoundException : Exception
{
    /// <summary>
    /// Идентификатор фильма, который не был найден.
    /// </summary>
    public Guid FilmId { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="filmId">Идентификатор фильма.</param>
    public FilmNotFoundException(Guid filmId) : base($"Film with ID {filmId} not found.")
    {
        FilmId = filmId;
    }
}