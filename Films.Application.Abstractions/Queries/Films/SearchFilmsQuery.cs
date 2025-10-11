using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Films;
using MediatR;

namespace Films.Application.Abstractions.Queries.Films;

/// <summary>
/// Запрос для поиска фильмов с фильтрами и пагинацией
/// </summary>
public class SearchFilmsQuery : IRequest<CountResult<FilmShortDto>>
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
    /// Фильтр по стране
    /// </summary>
    public string? Country { get; init; }
    
    /// <summary>
    /// Фильтр по имени участника (актер, режиссер)
    /// </summary>
    public string? Person { get; init; }
    
    /// <summary>
    /// Фильтр по типу контента (сериал/фильм)
    /// </summary>
    public bool? Serial { get; init; }
    
    /// <summary>
    /// Минимальный год выпуска
    /// </summary>
    public int? MinYear { get; init; }
    
    /// <summary>
    /// Максимальный год выпуска
    /// </summary>
    public int? MaxYear { get; init; }
    
    /// <summary>
    /// Фильтр по плейлисту
    /// </summary>
    public Guid? PlaylistId { get; init; }
    
    /// <summary>
    /// Количество пропускаемых результатов
    /// </summary>
    public required int Skip { get; init; }
    
    /// <summary>
    /// Количество получаемых результатов
    /// </summary>
    public required int Take { get; init; }
}