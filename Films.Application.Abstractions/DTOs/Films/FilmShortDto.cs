namespace Films.Application.Abstractions.DTOs.Films;

/// <summary>
/// DTO для передачи кратких данных о фильме
/// </summary>
public class FilmShortDto
{
    /// <summary>
    /// Уникальный идентификатор фильма
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Название фильма
    /// </summary>
    public required string Title { get; init; }
    
    /// <summary>
    /// Ключ постера фильма в хранилище
    /// </summary>
    public required string PosterKey { get; init; }
    
    /// <summary>
    /// Год выпуска
    /// </summary>
    public required int Year { get; init; }
    
    /// <summary>
    /// Рейтинг КиноПоиска (может быть null)
    /// </summary>
    public double? RatingKp { get; init; }
    
    /// <summary>
    /// Рейтинг IMDb (может быть null)
    /// </summary>
    public double? RatingImdb { get; init; }
    
    /// <summary>
    /// Описание фильма
    /// </summary>
    public required string Description { get; init; }
    
    /// <summary>
    /// Флаг, указывающий является ли контент сериалом
    /// </summary>
    public required bool IsSerial { get; init; }
    
    /// <summary>
    /// Список жанров
    /// </summary>
    public required IReadOnlyList<string> Genres { get; init; }
}