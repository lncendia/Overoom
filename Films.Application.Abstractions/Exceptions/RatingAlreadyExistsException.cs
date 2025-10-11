namespace Films.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, возникающее при попытке создать оценку для фильма от пользователя, который уже оценил этот фильм.
/// </summary>
public class RatingAlreadyExistsException : Exception
{
    /// <summary>
    /// Идентификатор фильма.
    /// </summary>
    public Guid FilmId { get; }

    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="filmId">Идентификатор фильма.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    public RatingAlreadyExistsException(Guid filmId, Guid userId)
        : base($"User {userId} has already rated film {filmId}. A user can only rate a film once.")
    {
        FilmId = filmId;
        UserId = userId;
    }
}