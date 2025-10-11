using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Playlists;
using MediatR;

namespace Films.Application.Abstractions.Queries.Playlists;


/// <summary>
/// Запрос для поиска плейлистов с фильтрами и пагинацией
/// </summary>
public class SearchPlaylistsQuery : IRequest<CountResult<PlaylistDto>>
{
    /// <summary>
    /// Поисковый запрос по названию
    /// </summary>
    public string? Query { get; init; }
    
    /// <summary>
    /// Фильтр по жанру
    /// </summary>
    public string? Genre { get; init; }
    
    /// <summary>
    /// Фильтр по идентификатору фильма
    /// </summary>
    public Guid? FilmId { get; init; }
    
    /// <summary>
    /// Количество пропускаемых результатов
    /// </summary>
    public required int Skip { get; init; }
    
    /// <summary>
    /// Количество получаемых результатов
    /// </summary>
    public required int Take { get; init; }
}