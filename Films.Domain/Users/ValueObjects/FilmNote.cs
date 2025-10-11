namespace Films.Domain.Users.ValueObjects;

/// <summary>
/// Класс FilmNote представляет запись заметки к фильму.
/// </summary>
public record FilmNote
{
    /// <summary>
    /// Идентификатор фильма.
    /// </summary>
    public required Guid FilmId { get; init; }

    /// <summary>
    /// Дата и время создания заметки.
    /// </summary>
    public DateTime Date { get; init; } = DateTime.UtcNow;
}