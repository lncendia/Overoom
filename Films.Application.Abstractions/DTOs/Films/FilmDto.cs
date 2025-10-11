using Films.Domain.Films.ValueObjects;

namespace Films.Application.Abstractions.DTOs.Films;

/// <summary>
/// DTO для передачи полных данных о фильме
/// </summary>
public class FilmDto : FilmShortDto
{
    /// <summary>
    /// Флаг, может ли быть создана комната с этим фильмом.
    /// </summary>
    public bool CanCreateRoom { get; init; }
    
    /// <summary>
    /// Рейтинг пользователя (может быть null)
    /// </summary>
    public double? UserRating { get; init; }
    
    /// <summary>
    /// Количество оценок пользователей
    /// </summary>
    public required int UserRatingsCount { get; init; }
    
    /// <summary>
    /// Общий пользовательский счет (может быть null)
    /// </summary>
    public double? UserScore { get; init; }
    
    /// <summary>
    /// Флаг наличия в списке ожидания пользователя (может быть null)
    /// </summary>
    public bool? InWatchlist { get; init; }
    
    /// <summary>
    /// Медиа контент фильма (может быть null)
    /// </summary>
    public MediaContentDto? Content { get; init; }
    
    /// <summary>
    /// Список сезонов (для сериалов, может быть null)
    /// </summary>
    public IReadOnlyList<SeasonDto>? Seasons { get; init; }
    
    /// <summary>
    /// Список стран производства
    /// </summary>
    public required IReadOnlyList<string> Countries { get; init; }
    
    /// <summary>
    /// Список режиссеров
    /// </summary>
    public required IReadOnlyList<string> Directors { get; init; }
    
    /// <summary>
    /// Список сценаристов
    /// </summary>
    public required IReadOnlyList<string> ScreenWriters { get; init; }
    
    /// <summary>
    /// Список актеров
    /// </summary>
    public required IReadOnlyList<Actor> Actors { get; init; }
}