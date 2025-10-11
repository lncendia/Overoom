namespace Films.Application.Abstractions.DTOs.Playlists;

/// <summary>
/// DTO для передачи данных плейлиста
/// </summary>
public class PlaylistDto
{
    /// <summary>
    /// Уникальный идентификатор плейлиста
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Название плейлиста
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Список жанров в плейлисте
    /// </summary>
    public required IReadOnlyList<string> Genres { get; init; }
    
    /// <summary>
    /// Описание плейлиста
    /// </summary>
    public required string Description { get; init; }
    
    /// <summary>
    /// Ключ постера плейлиста в хранилище
    /// </summary>
    public required string PosterKey { get; init; }
    
    /// <summary>
    /// Дата и время последнего обновления
    /// </summary>
    public required DateTime Updated { get; init; }
}