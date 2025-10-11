namespace Films.Domain.Rooms.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда фильм недоступен для просмотра.
/// </summary>
public class FilmNotAvailableException : Exception
{
    /// <summary>
    /// Идентификатор фильма, который недоступен.
    /// </summary>
    public Guid FilmId { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="filmId">Идентификатор фильма.</param>
    public FilmNotAvailableException(Guid filmId) 
        : base($"Film with ID {filmId} is not available. Content has not been uploaded.")
    {
        FilmId = filmId;
    }
}