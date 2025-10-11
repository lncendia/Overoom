using Films.Infrastructure.Web.Components.Interfaces;

namespace Films.Infrastructure.Web.Playlists.InputModels;

/// <summary>
/// Модель данных для поиска плейлистов с фильтрами и пагинацией
/// </summary>
public class SearchPlaylistsInputModel : IWithInputPagination
{
    /// <summary>
    /// Поисковый запрос (название плейлиста)
    /// </summary>
    public string? Query { get; init; }

    /// <summary>
    /// Жанр для фильтрации плейлистов
    /// </summary>
    public string? Genre { get; init; }

    /// <summary>
    /// Идентификатор фильма для поиска плейлистов, содержащих этот фильм
    /// </summary>
    public Guid? FilmId { get; init; }

    /// <summary>
    /// Количество элементов на странице (по умолчанию 10, максимум 50)
    /// </summary>
    public int Take { get; init; } = 10;

    /// <summary>
    /// Количество пропускаемых элементов
    /// </summary>
    public int Skip { get; init; } = 0;
}