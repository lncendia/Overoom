using Films.Infrastructure.Web.Components.Interfaces;

namespace Films.Infrastructure.Web.Films.InputModels;

/// <summary>
/// Модель данных для поиска фильмов с фильтрами и пагинацией
/// </summary>
public class SearchFilmsInputModel : IWithInputPagination
{
    /// <summary>
    /// Поисковый запрос (название фильма)
    /// </summary>
    public string? Query { get; init; }

    /// <summary>
    /// Жанр для фильтрации
    /// </summary>
    public string? Genre { get; init; }

    /// <summary>
    /// Участник фильма (актер, режиссер и т.д.)
    /// </summary>
    public string? Person { get; init; }

    /// <summary>
    /// Страна производства
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// Признак сериала (true - только сериалы, false - только фильмы, null - все)
    /// </summary>
    public bool? Serial { get; init; }

    /// <summary>
    /// Идентификатор плейлиста для поиска
    /// </summary>
    public Guid? PlaylistId { get; init; }

    /// <summary>
    /// Минимальный год выпуска
    /// </summary>
    public int? MinYear { get; init; }

    /// <summary>
    /// Максимальный год выпуска
    /// </summary>
    public int? MaxYear { get; init; }

    /// <summary>
    /// Количество элементов на странице (по умолчанию 10, максимум 50)
    /// </summary>
    public int Take { get; init; } = 10;

    /// <summary>
    /// Количество пропускаемых элементов
    /// </summary>
    public int Skip { get; init; } = 0;
}