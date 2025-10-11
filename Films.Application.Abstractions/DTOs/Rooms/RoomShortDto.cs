namespace Films.Application.Abstractions.DTOs.Rooms;

/// <summary>
/// DTO для передачи кратких данных о комнате с информацией о фильме
/// </summary>
public class RoomShortDto
{
    /// <summary>
    /// Уникальный идентификатор комнаты
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Количество зрителей в комнате
    /// </summary>
    public required int ViewersCount { get; init; }
    
    /// <summary>
    /// Флаг, указывающий является ли комната приватной
    /// </summary>
    public required bool IsPrivate { get; init; }
    
    /// <summary>
    /// Идентификатор фильма, который воспроизводится в комнате
    /// </summary>
    public required Guid FilmId { get; init; }
    
    /// <summary>
    /// Название фильма
    /// </summary>
    public required string Title { get; init; }
    
    /// <summary>
    /// Ключ постера фильма в хранилище
    /// </summary>
    public required string PosterKey { get; init; }
    
    /// <summary>
    /// Год выпуска фильма
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
    /// Список жанров фильма
    /// </summary>
    public required IReadOnlyList<string> Genres { get; init; }
}